using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TimeoutPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically to check that the <see cref="ITimeoutPolicyEventHandler"/> is triggered
    /// when the timeout policy on timeout event is triggered.
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
    [Collection(XUnitTestCollections.TimeoutPolicy)]
    public class AddTimeoutPolicyEventHandlerTests : IDisposable
    {
        /// <summary>
        /// Tests that the overloads of TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// do not take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultTimeoutPolicyEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ITimeoutPolicyEventHandler.OnTimeoutAsync"/> but it does assert that
        /// the onTimeoutAsync event from the policy is linked to the <see cref="DefaultTimeoutPolicyEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the timeout policy that indeed
        /// the  <see cref="DefaultTimeoutPolicyEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyShouldTriggerDefaultEventHandler()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 2
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var timeoutPolicyAsserter = new TimeoutPolicyAsserter(
                httpClientName,
                timeoutOptions,
                timeoutPolicy);
            timeoutPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ITimeoutPolicyEventHandler.OnTimeoutAsync"/> but it does assert that
        /// the onTimeoutAsync event from the policy is linked to expected type.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyShouldTriggerCustomEventHandler()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 2
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var timeoutPolicyAsserter = new TimeoutPolicyAsserter(
                httpClientName,
                timeoutOptions,
                timeoutPolicy);
            timeoutPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test triggers the timeout policy to make sure the <see cref="TimeoutEvent"/> is triggered
        /// as expected.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicyTriggersCustomEventHandler()
        {
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05 //50ms
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            // this timeout is a max timeout before aborting
                            // but the polly timeout policy will timeout before this happens
                            builder.TimesOut(TimeSpan.FromSeconds(1));
                        });
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await Should.ThrowAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://github.com"));

            TestTimeoutPolicyEventHandler.OnTimeoutAsyncCalls.Count.ShouldBe(1);
            var timeoutEvent = TestTimeoutPolicyEventHandler.OnTimeoutAsyncCalls.First();
            timeoutEvent.HttpClientName.ShouldBe(httpClientName);
            timeoutEvent.TimeoutOptions.TimeoutInSecs.ShouldBe(timeoutOptions.TimeoutInSecs);
        }

        public void Dispose()
        {
            TestTimeoutPolicyEventHandler.Clear();
        }
    }
}
