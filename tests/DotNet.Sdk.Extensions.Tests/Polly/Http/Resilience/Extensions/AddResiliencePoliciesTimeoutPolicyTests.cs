using System;
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
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to test that the timeout policy is added.
    ///
    /// Because the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies overload methods also add
    /// a fallback policy that handles timeouts we need to assert that the timeout will result in the
    /// fallback policy response.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesTimeoutPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy1()
        {
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
                    RetryCount = 0, //disable retries to avoid the retry policy triggering when testing the timeout policy
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the timeout policy
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
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Timeout
                .HttpClientShouldContainTimeoutPolicyWithFallbackAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy2()
        {
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
                    RetryCount = 0, //disable retries to avoid the retry policy triggering when testing the timeout policy
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the timeout policy
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
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Timeout
                .HttpClientShouldContainTimeoutPolicyWithFallbackAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the  <see cref="IResiliencePoliciesEventHandler.OnTimeoutAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
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
                    RetryCount = 0, //disable retries to avoid the retry policy triggering when testing the timeout policy
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the timeout policy
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
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout
                .HttpClientShouldContainTimeoutPolicyWithFallbackAsync();
            resiliencePoliciesAsserter.Timeout
                .EventHandlerShouldReceiveExpectedEvents(
                    count: 1,
                    httpClientName: httpClientName,
                    eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the  <see cref="IResiliencePoliciesEventHandler.OnTimeoutAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
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
                    RetryCount = 0, //disable retries to avoid the retry policy triggering when testing the timeout policy
                    MedianFirstRetryDelayInSecs = 0.01
                },
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 1000 // configure high value to avoid the circuit breaker policy triggering when testing the timeout policy
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
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout
                .HttpClientShouldContainTimeoutPolicyWithFallbackAsync();
            resiliencePoliciesAsserter.Timeout
                .EventHandlerShouldReceiveExpectedEvents(
                    count: 1,
                    httpClientName: httpClientName,
                    eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }
    }
}
