using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Extensions
{
    /// <summary>
    /// Tests for the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically to check that the <see cref="ICircuitBreakerPolicyEventHandler"/> is triggered
    /// when the circuit breaker policy on retry event is triggered.
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
    public class AddCircuitBreakerPolicyEventHandlerTests
    {
        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy that
        /// do not take in a <see cref="ICircuitBreakerPolicyEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultCircuitBreakerPolicyEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnBreakAsync"/>,
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnResetAsync"/>, or
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnHalfOpenAsync"/> but it does assert that
        /// the onRetryAsync event from the policy is linked to the <see cref="DefaultCircuitBreakerPolicyEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the circuit breaker policy that indeed
        /// the  <see cref="DefaultCircuitBreakerPolicyEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicyShouldTriggerDefaultEventHandler()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyEventHandler: typeof(DefaultCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy that
        /// take in a <see cref="ICircuitBreakerPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnBreakAsync"/>,
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnResetAsync"/>, or
        /// <see cref="ICircuitBreakerPolicyEventHandler.OnHalfOpenAsync"/> but it does assert that
        /// these events from the policy are linked to expected type.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicyShouldTriggerCustomEventHandler()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyEventHandler: typeof(TestCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy that
        /// take in a <see cref="ICircuitBreakerPolicyEventHandler"/> type should have their events handled by
        /// that type.
        ///
        /// This test triggers the circuit breaker policy to make sure the <see cref="BreakEvent"/> is triggered
        /// as expected.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicyTriggersCustomEventHandler()
        {
            AsyncPolicyWrap<HttpResponseMessage>? wrappedCircuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 1;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            builder.RespondWith(httpRequestMessage=>
                            {
                                return httpRequestMessage.RequestUri!.PathAndQuery.Contains("/ok") 
                                    ? new HttpResponseMessage(HttpStatusCode.OK)
                                    : new HttpResponseMessage(HttpStatusCode.InternalServerError);
                            });
                        });
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    wrappedCircuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                }); ;


            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var circuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;

            // force isolate should trigger OnBreak
            circuitBreakerPolicy.Isolate();
            TestCircuitBreakerPolicyEventHandler.OnBreakAsyncCalls.Count.ShouldBe(1);
            TestCircuitBreakerPolicyEventHandler.OnResetAsyncCalls.Count.ShouldBe(0);
            TestCircuitBreakerPolicyEventHandler.OnHalfOpenAsyncCalls.Count.ShouldBe(0);

            // force reset should trigger OnReset
            circuitBreakerPolicy.Reset();
            TestCircuitBreakerPolicyEventHandler.OnBreakAsyncCalls.Count.ShouldBe(1);
            TestCircuitBreakerPolicyEventHandler.OnResetAsyncCalls.Count.ShouldBe(1);
            TestCircuitBreakerPolicyEventHandler.OnHalfOpenAsyncCalls.Count.ShouldBe(0);

            // trigger the OnBreak by triggering the circuit breaker
            for (var i = 0; i < minimumThroughput; i++)
            {
                await httpClient.GetAsync("https://github.com/fail");
            }
            TestCircuitBreakerPolicyEventHandler.OnBreakAsyncCalls.Count.ShouldBe(2);
            TestCircuitBreakerPolicyEventHandler.OnResetAsyncCalls.Count.ShouldBe(1);
            TestCircuitBreakerPolicyEventHandler.OnHalfOpenAsyncCalls.Count.ShouldBe(0);

            // wait for the circuit to reset to trigger the OnReset and
            // put some successful traffic to trigger the OnHalfOpen
            await Task.Delay(TimeSpan.FromSeconds(durationOfBreakInSecs + 1));
            for (var i = 0; i < minimumThroughput; i++)
            {
                await httpClient.GetAsync("https://github.com/ok");
            }
            TestCircuitBreakerPolicyEventHandler.OnBreakAsyncCalls.Count.ShouldBe(2);
            TestCircuitBreakerPolicyEventHandler.OnResetAsyncCalls.Count.ShouldBe(2);
            TestCircuitBreakerPolicyEventHandler.OnHalfOpenAsyncCalls.Count.ShouldBe(1);

            // assert some properties on the events
            TestCircuitBreakerPolicyEventHandler.OnBreakAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(durationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(failureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(minimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(samplingDurationInSecs))
                .ShouldBe(2);
            TestCircuitBreakerPolicyEventHandler.OnResetAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(durationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(failureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(minimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(samplingDurationInSecs))
                .ShouldBe(2);
            TestCircuitBreakerPolicyEventHandler.OnHalfOpenAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(durationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(failureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(minimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(samplingDurationInSecs))
                .ShouldBe(1);
        }
    }
}
