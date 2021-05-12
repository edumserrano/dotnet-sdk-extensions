using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
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
    /// Specifically to test that the retry policy is added.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesRetryPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy1()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.05
                },
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.2,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the retry policy
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
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
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClient,
                resilienceOptions,
                testHttpMessageHandler);
            await resiliencePoliciesAsserter
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.05
                },
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.2,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the retry policy
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
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClient,
                resilienceOptions,
                testHttpMessageHandler);
            await resiliencePoliciesAsserter
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.05
                },
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.2,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the retry policy
                }
            };
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
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
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClient,
                resilienceOptions,
                testHttpMessageHandler);
            await resiliencePoliciesAsserter
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            resiliencePoliciesAsserter
                .Retry
                .EventHandlerShouldReceiveExpectedEvents(
                    count: 15 * resilienceOptions.Retry.RetryCount, // the resiliencePoliciesAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
                    httpClientName: httpClientName,
                    eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Retry);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.05
                },
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.2,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the retry policy
                }
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
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
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = new ResiliencePoliciesAsserter(
                httpClient,
                resilienceOptions,
                testHttpMessageHandler);
            await resiliencePoliciesAsserter
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            resiliencePoliciesAsserter
                .Retry
                .EventHandlerShouldReceiveExpectedEvents(
                    count: 15 * resilienceOptions.Retry.RetryCount, // the resiliencePoliciesAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
                    httpClientName: httpClientName,
                    eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Retry);
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
    }
}
