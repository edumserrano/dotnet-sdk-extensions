using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically for the ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies overloads.
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
    public class AddResiliencePoliciesTests : IDisposable
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="ResilienceOptions"/>.
        /// </summary>
        [Fact]
        public void AddResiliencePolicies1()
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
            resiliencePoliciesAsserter.PoliciesShouldBeConfiguredAsExpected();
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(DefaultResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts only the name of the option to use for the value of the <see cref="ResilienceOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddResiliencePolicies2()
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
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(optionsName)
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
            resiliencePoliciesAsserter.PoliciesShouldBeConfiguredAsExpected();
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(DefaultResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts the name of the option to use for the value of the <see cref="ResilienceOptions"/>
        /// and a <see cref="IRetryPolicyEventHandler"/> type to handle the retry policy events.
        /// </summary>
        [Fact]
        public void AddResiliencePolicies3()
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
            resiliencePoliciesAsserter.PoliciesShouldBeConfiguredAsExpected();
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(TestResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts the name of the option to use for the value of the <see cref="ResilienceOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method.
        ///
        /// This overload also accepts an <see cref="IRetryPolicyEventHandler"/> type to handle the retry policy events.
        /// </summary>
        [Fact]
        public void AddResiliencePolicies4()
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
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
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
            resiliencePoliciesAsserter.PoliciesShouldBeConfiguredAsExpected();
            resiliencePoliciesAsserter.PoliciesShouldTriggerPolicyEventHandler(typeof(TestResiliencePoliciesEventHandler));
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesUniquePolicyPerHttpClient()
        {
            var policyHttpMessageHandlers1 = new List<PolicyHttpMessageHandler>();
            var policyHttpMessageHandlers2 = new List<PolicyHttpMessageHandler>();
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers1 = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });
            services
                .AddHttpClient("Microsoft")
                .AddResiliencePolicies(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers2 = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            var resiliencePolicies1 = new ResiliencePolicies(policyHttpMessageHandlers1);
            resiliencePolicies1.TimeoutPolicy.ShouldNotBeNull();
            resiliencePolicies1.RetryPolicy.ShouldNotBeNull();
            resiliencePolicies1.CircuitBreakerPolicy.ShouldNotBeNull();
            resiliencePolicies1.FallbackPolicy.ShouldNotBeNull();
            
            var resiliencePolicies2 = new ResiliencePolicies(policyHttpMessageHandlers2);
            resiliencePolicies2.TimeoutPolicy.ShouldNotBeNull();
            resiliencePolicies2.RetryPolicy.ShouldNotBeNull();
            resiliencePolicies2.CircuitBreakerPolicy.ShouldNotBeNull();
            resiliencePolicies2.FallbackPolicy.ShouldNotBeNull();
            
            ReferenceEquals(resiliencePolicies1.TimeoutPolicy, resiliencePolicies2.TimeoutPolicy).ShouldBeFalse();
            resiliencePolicies1.TimeoutPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.TimeoutPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.RetryPolicy, resiliencePolicies2.RetryPolicy).ShouldBeFalse();
            resiliencePolicies1.RetryPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.RetryPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.CircuitBreakerPolicy, resiliencePolicies2.CircuitBreakerPolicy).ShouldBeFalse();
            resiliencePolicies1.CircuitBreakerPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.CircuitBreakerPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.FallbackPolicy, resiliencePolicies2.FallbackPolicy).ShouldBeFalse();
            resiliencePolicies1.FallbackPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.FallbackPolicy.PolicyKey);
        }

        public void Dispose()
        {
            TestResiliencePoliciesEventHandler.Clear();
        }
    }
}
