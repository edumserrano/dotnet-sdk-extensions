using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to check that the <see cref="IResiliencePoliciesEventHandler"/> is triggered
    /// when the any of the resilience policies events are triggered.
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
    public class AddResiliencePoliciesEventHandlerTests : IDisposable
    {
        /// <summary>
        /// Tests that the overloads of ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies that
        /// do not take in a <see cref="IResiliencePoliciesEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultResiliencePoliciesEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> but it does assert that
        /// the onRetryAsync event from the policy is linked to the <see cref="DefaultResiliencePoliciesEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the retry policy that indeed
        /// the  <see cref="DefaultResiliencePoliciesEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesShouldTriggerDefaultEventHandler()
        {
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 1
                },
                Retry = new RetryOptions
                {
                    RetryCount = 3,
                    MedianFirstRetryDelayInSecs = 1
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 1,
                    FailureThreshold = 0.5,
                    SamplingDurationInSecs = 60,
                    MinimumThroughput = 4
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClientName,
                resilienceOptions,
                policyHttpMessageHandlers);
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(DefaultResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies that
        /// take in a <see cref="IResiliencePoliciesEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> but it does assert that
        /// the onRetryAsync event from the policy is linked to expected type.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesShouldTriggerCustomEventHandler()
        {
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 1
                },
                Retry = new RetryOptions
                {
                    RetryCount = 3,
                    MedianFirstRetryDelayInSecs = 1
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 1,
                    FailureThreshold = 0.5,
                    SamplingDurationInSecs = 60,
                    MinimumThroughput = 4
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClientName,
                resilienceOptions,
                policyHttpMessageHandlers);
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(TestResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies that
        /// take in a <see cref="IResiliencePoliciesEventHandler"/> type should have their events handled by that type.
        ///
        /// This test triggers the retry policy to make sure the <see cref="RetryEvent"/> is triggered
        /// as expected.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesTriggersCustomEventHandlerForRetry()
        {
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 1
                },
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.05 // 50 ms
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 1,
                    FailureThreshold = 0.5,
                    SamplingDurationInSecs = 60,
                    MinimumThroughput = 4
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            // always return 500 to trigger the retry policy
                            builder.RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                        });
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient.GetAsync("https://github.com");
            
            TestRetryPolicyEventHandler.OnRetryAsyncCalls.Count.ShouldBe(resilienceOptions.Retry.RetryCount);
            foreach (var retryEvent in TestRetryPolicyEventHandler.OnRetryAsyncCalls)
            {
                retryEvent.HttpClientName.ShouldBe(httpClientName);
                retryEvent.RetryOptions.RetryCount.ShouldBe(resilienceOptions.Retry.RetryCount);
                retryEvent.RetryOptions.MedianFirstRetryDelayInSecs.ShouldBe(resilienceOptions.Retry.MedianFirstRetryDelayInSecs);
            }
        }

        public void Dispose()
        {
            TestResiliencePoliciesEventHandler.Clear();
        }
    }
}
