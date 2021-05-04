using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
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
    /// Specifically for the FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy overloads.
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
    public class AddFallbackPolicyTests : IDisposable
    {
        /// <summary>
        /// Tests that the <see cref="FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        ///
        /// This overload uses the default <see cref="DefaultFallbackPolicyEventHandler"/> to handle fallback events.
        /// </summary>
        [Fact]
        public void AddFallbackPolicy1()
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
        /// Tests that the <see cref="FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy{TPolicyEventHandler}"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts a <see cref="IFallbackPolicyEventHandler"/> type to handle the fallback policy events.
        /// </summary>
        [Fact]
        public void AddFallbackPolicy3()
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
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddFallbackPolicyUniquePolicyPerHttpClient()
        {
            AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy1 = null;
            AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddFallbackPolicy()
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    fallbackPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddFallbackPolicy()
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    fallbackPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            fallbackPolicy1.ShouldNotBeNull();
            fallbackPolicy2.ShouldNotBeNull();
            ReferenceEquals(fallbackPolicy1, fallbackPolicy2).ShouldBeFalse();
            fallbackPolicy1.PolicyKey.ShouldNotBe(fallbackPolicy2.PolicyKey);
        }

        [Fact]
        public async Task AddFallbackPolicyTriggersCustomEventHandler()
        {
            var httpClientName = "GitHub";
            var timeoutRejectedException = new TimeoutRejectedException();
            var brokenCircuitException = new BrokenCircuitException();
            var isolatedCircuitException = new IsolatedCircuitException("");
            var taskCanceledException = new TaskCanceledException();
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
                                return httpRequestMessage.RequestUri!.PathAndQuery switch
                                {
                                    { } path when path.Contains("/timeout") => throw timeoutRejectedException,
                                    { } path when path.Contains("/brokenCircuit") => throw brokenCircuitException,
                                    { } path when path.Contains("/isolatedCircuit") => throw isolatedCircuitException,
                                    { } path when path.Contains("/aborted") => throw taskCanceledException,
                                    _ => new HttpResponseMessage(HttpStatusCode.OK)
                                };
                            });
                        });
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);

            // trigger timeout fallback policy
            var timeoutHttpResponseMessage = await httpClient.GetAsync("https://github.com/timeout") as TimeoutHttpResponseMessage;
            timeoutHttpResponseMessage.ShouldNotBeNull();
            ReferenceEquals(timeoutHttpResponseMessage.Exception, timeoutRejectedException).ShouldBe(true);

            // trigger circuit broken fallback policy 1
            var circuitBrokenHttpResponseMessage1 = await httpClient.GetAsync("https://github.com/brokenCircuit") as CircuitBrokenHttpResponseMessage;
            circuitBrokenHttpResponseMessage1.ShouldNotBeNull();
            circuitBrokenHttpResponseMessage1.IsolatedCircuitException.ShouldBeNull();
            circuitBrokenHttpResponseMessage1.CircuitBreakerState.ShouldBe(CircuitBreakerState.Open);
            ReferenceEquals(circuitBrokenHttpResponseMessage1.BrokenCircuitException, brokenCircuitException).ShouldBe(true);

            // trigger circuit broken fallback policy 2
            var circuitBrokenHttpResponseMessage2 = await httpClient.GetAsync("https://github.com/isolatedCircuit") as CircuitBrokenHttpResponseMessage;
            circuitBrokenHttpResponseMessage2.ShouldNotBeNull();
            circuitBrokenHttpResponseMessage2.BrokenCircuitException.ShouldBeNull();
            circuitBrokenHttpResponseMessage2.CircuitBreakerState.ShouldBe(CircuitBreakerState.Isolated);
            ReferenceEquals(circuitBrokenHttpResponseMessage2.IsolatedCircuitException, isolatedCircuitException).ShouldBe(true);

            // trigger aborted fallback policy
            var abortedHttpResponseMessage = await httpClient.GetAsync("https://github.com/aborted") as AbortedHttpResponseMessage;
            abortedHttpResponseMessage.ShouldNotBeNull();
            abortedHttpResponseMessage.TriggeredByTimeoutException.ShouldBeFalse();
            ReferenceEquals(abortedHttpResponseMessage.Exception, taskCanceledException).ShouldBe(true);
        }

        public void Dispose()
        {
            TestFallbackPolicyEventHandler.Clear();
        }
    }
}
