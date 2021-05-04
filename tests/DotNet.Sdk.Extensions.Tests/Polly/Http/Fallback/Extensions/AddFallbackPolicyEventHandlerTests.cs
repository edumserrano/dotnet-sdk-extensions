using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Extensions
{
    /// <summary>
    /// Tests for the <see cref="FallbackPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically to check that the <see cref="IFallbackPolicyEventHandler"/> is triggered
    /// when the fallback policy on retry event is triggered.
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
    [Collection(XUnitTestCollections.FallbackPolicy)]
    public class AddFallbackPolicyEventHandlerTests : IDisposable
    {
        /// <summary>
        /// Tests that the overloads of FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy that
        /// do not take in a <see cref="IFallbackPolicyEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultFallbackPolicyEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IFallbackPolicyEventHandler.OnTimeoutFallbackAsync"/>,
        /// <see cref="IFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsync"/> and
        /// <see cref="IFallbackPolicyEventHandler.OnTaskCancelledFallbackAsync"/> but it does assert that
        /// the events from the policy are linked to the <see cref="DefaultFallbackPolicyEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the fallback policy that indeed
        /// the  <see cref="DefaultFallbackPolicyEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddFallbackPolicyShouldTriggerDefaultEventHandler()
        {
            AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy = null;
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddFallbackPolicy()
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    fallbackPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var fallbackPolicyAsserter = new FallbackPolicyAsserter(httpClientName, fallbackPolicy);
            fallbackPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            fallbackPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultFallbackPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy that
        /// take in a <see cref="IFallbackPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IFallbackPolicyEventHandler.OnTimeoutFallbackAsync"/>,
        /// <see cref="IFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsync"/> and
        /// <see cref="IFallbackPolicyEventHandler.OnTaskCancelledFallbackAsync"/> but it does assert that
        /// the events from the policy are linked to expected type.
        /// </summary>
        [Fact]
        public void AddFallbackPolicyShouldTriggerCustomEventHandler()
        {
            AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy = null;
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddFallbackPolicy<TestFallbackPolicyEventHandler>()
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    fallbackPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var fallbackPolicyAsserter = new FallbackPolicyAsserter(httpClientName, fallbackPolicy);
            fallbackPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            fallbackPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestFallbackPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy that
        /// take in a <see cref="IFallbackPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test triggers the fallback policy to make sure the <see cref="TimeoutFallbackEvent"/>,
        /// <see cref="BrokenCircuitFallbackEvent"/> and <see cref="TaskCancelledFallbackEvent"/> are triggered as expected.
        /// </summary>
        [Fact]
        public async Task AddFallbackPolicyTriggersCustomEventHandler()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddFallbackPolicy<TestFallbackPolicyEventHandler>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            builder.RespondWith(httpRequestMessage =>
                            {
                                return httpRequestMessage.RequestUri switch
                                {
                                    { PathAndQuery: { } path } when path.Contains("/timeout") => throw new TimeoutRejectedException(),
                                    { PathAndQuery: { } path } when path.Contains("/brokenCircuit") => throw new BrokenCircuitException(),
                                    { PathAndQuery: { } path } when path.Contains("/isolatedCircuit") => throw new IsolatedCircuitException(""),
                                    { PathAndQuery: { } path } when path.Contains("/aborted") => throw new TaskCanceledException(),
                                    _ => new HttpResponseMessage(HttpStatusCode.OK)
                                };
                            });
                        });
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            
            // trigger timeout fallback policy
            await httpClient.GetAsync("https://github.com/timeout");
            TestFallbackPolicyEventHandler.OnTimeoutFallbackAsyncCalls.Count.ShouldBe(1);
            TestFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsyncAsyncCalls.Count.ShouldBe(0);
            TestFallbackPolicyEventHandler.OnTaskCancelledFallbackAsyncCalls.Count.ShouldBe(0);

            // trigger circuit broken fallback policy 1
            await httpClient.GetAsync("https://github.com/brokenCircuit");
            TestFallbackPolicyEventHandler.OnTimeoutFallbackAsyncCalls.Count.ShouldBe(1);
            TestFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsyncAsyncCalls.Count.ShouldBe(1);
            TestFallbackPolicyEventHandler.OnTaskCancelledFallbackAsyncCalls.Count.ShouldBe(0);

            // trigger circuit broken fallback policy 2
            await httpClient.GetAsync("https://github.com/isolatedCircuit");
            TestFallbackPolicyEventHandler.OnTimeoutFallbackAsyncCalls.Count.ShouldBe(1);
            TestFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsyncAsyncCalls.Count.ShouldBe(2);
            TestFallbackPolicyEventHandler.OnTaskCancelledFallbackAsyncCalls.Count.ShouldBe(0);

            // trigger aborted fallback policy
            await httpClient.GetAsync("https://github.com/aborted");
            TestFallbackPolicyEventHandler.OnTimeoutFallbackAsyncCalls.Count.ShouldBe(1);
            TestFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsyncAsyncCalls.Count.ShouldBe(2);
            TestFallbackPolicyEventHandler.OnTaskCancelledFallbackAsyncCalls.Count.ShouldBe(1);

            // assert some properties on the events
            TestFallbackPolicyEventHandler.OnTimeoutFallbackAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName))
                .ShouldBe(1);
            TestFallbackPolicyEventHandler.OnBrokenCircuitFallbackAsyncAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName))
                .ShouldBe(2);
            TestFallbackPolicyEventHandler.OnTaskCancelledFallbackAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName))
                .ShouldBe(1);
        }

        public void Dispose()
        {
            TestFallbackPolicyEventHandler.Clear();
        }
    }
}
