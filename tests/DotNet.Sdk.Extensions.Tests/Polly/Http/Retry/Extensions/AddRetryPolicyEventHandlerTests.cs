using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically to check that the <see cref="IRetryPolicyEventHandler"/> is triggered
    /// when the retry policy on retry event is triggered.
    /// 
    /// Many tests here use reflection to check that the policy is configured as expected.
    /// Although I'd prefer to do it without using reflection I couldn't find an alternative.
    /// At least not one that wouldn't force me to trigger the policy in different scenarios
    /// to check what I need. If I did that then it would almost be like testing that the Polly
    /// policies do what they are supposed to do and my intention is NOT to test the Polly code.
    ///
    /// Because of the reflection usage these tests can break when updating the Polly packages.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddRetryPolicyEventHandlerTests
    {
        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy that
        /// do not take in a <see cref="IRetryPolicyEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultRetryPolicyEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IRetryPolicyEventHandler.OnRetryAsync"/> but it does assert that
        /// the onRetryAsync event from the policy is linked to the <see cref="DefaultRetryPolicyEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the retry policy that indeed
        /// the  <see cref="DefaultRetryPolicyEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddRetryPolicyShouldTriggerDefaultEventHandler()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            retryPolicy.ShouldNotBeNull();
            retryPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                retryCount: retryCount,
                medianFirstRetryDelayInSecs: medianFirstRetryDelayInSecs,
                policyConfigurationType: typeof(DefaultRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy that
        /// take in a <see cref="IRetryPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IRetryPolicyEventHandler.OnRetryAsync"/> but it does assert that
        /// the onRetryAsync event from the policy is linked to expected type.
        /// </summary>
        [Fact]
        public void AddRetryPolicyShouldTriggerCustomEventHandler()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy<TestRetryPolicyEventHandler>(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            retryPolicy.ShouldNotBeNull();
            retryPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                retryCount: retryCount,
                medianFirstRetryDelayInSecs: medianFirstRetryDelayInSecs,
                policyConfigurationType: typeof(TestRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy that
        /// take in a <see cref="IRetryPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test triggers the retry policy to make sure the <see cref="RetryEvent"/> is triggered
        /// as expected.
        /// </summary>
        [Fact]
        public async Task AddRetryPolicyTriggersCustomEventHandler()
        {
            var httpClientName = "GitHub";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.05; //50 ms
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy<TestRetryPolicyEventHandler>(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            // always return 500 to trigger the retry policy
                            builder.RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                        });
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient.GetAsync("https://github.com");

            TestRetryPolicyEventHandler.OnRetryAsyncCalls.Count.ShouldBe(retryCount);
            foreach (var retryEvent in TestRetryPolicyEventHandler.OnRetryAsyncCalls)
            {
                retryEvent.HttpClientName.ShouldBe(httpClientName);
                retryEvent.RetryOptions.RetryCount.ShouldBe(retryCount);
                retryEvent.RetryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
            }
        }
    }
}
