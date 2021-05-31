using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    ///
    /// NOTE: when debugging sometimes the tests might not behave as expected because the circuit breaker
    /// is time sensitive in its nature as shown by the duration of break and sampling duration properties.
    /// If required try to increase the ResilienceOptions.SamplingDurationInSecs to a greater value to
    /// allow the tests to run successfully when the debugger is attached. For instance, set it to 2 instead of 0.2.
    ///
    /// This is not ideal but at the moment it's the only suggested workaround because these tests are triggering
    /// the circuit breaker policy and I don't know how to manipulate/fake time for the policy.
    ///
    /// Disables other policies to avoid triggering them when testing the circuit breaker policy.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesCircuitBreakerPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsCircuitBreakerPolicy1()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableTimeoutPolicy = false,
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 10
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
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
                .CircuitBreaker
                .HttpClientShouldContainCircuitBreakerPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsCircuitBreakerPolicy2()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableTimeoutPolicy = false,
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 10
                }
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
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
                .CircuitBreaker
                .HttpClientShouldContainCircuitBreakerPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the  <see cref="IResiliencePoliciesEventHandler.OnBreakAsync"/>,
        /// <see cref="IResiliencePoliciesEventHandler.OnResetAsync"/> and
        /// <see cref="IResiliencePoliciesEventHandler.OnHalfOpenAsync"/>,
        /// events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsCircuitBreakerPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableTimeoutPolicy = false,
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 10
                }
            };
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.CircuitBreaker.HttpClientShouldContainCircuitBreakerPolicyAsync();
            resiliencePoliciesAsserter.CircuitBreaker.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.CircuitBreaker);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the  <see cref="IResiliencePoliciesEventHandler.OnBreakAsync"/>,
        /// <see cref="IResiliencePoliciesEventHandler.OnResetAsync"/> and
        /// <see cref="IResiliencePoliciesEventHandler.OnHalfOpenAsync"/>,
        /// events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsCircuitBreakerPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableTimeoutPolicy = false,
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 10
                }
            };
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
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
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.CircuitBreaker.HttpClientShouldContainCircuitBreakerPolicyAsync();
            resiliencePoliciesAsserter.CircuitBreaker.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.CircuitBreaker);
        }

        /// <summary>
        /// Tests that the AddResiliencePolicies method does not add a circuit breaker policy if
        /// the <see cref="ResilienceOptions.EnableCircuitBreakerPolicy"/> option is false.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsCircuitBreakerPolicy5()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableCircuitBreakerPolicy = false,
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableTimeoutPolicy = false,
                CircuitBreaker = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = 0.1,
                    SamplingDurationInSecs = 0.3,
                    FailureThreshold = 0.6,
                    MinimumThroughput = 10
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.CircuitBreaker.DurationOfBreakInSecs = resilienceOptions.CircuitBreaker.DurationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = resilienceOptions.CircuitBreaker.FailureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = resilienceOptions.CircuitBreaker.SamplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = resilienceOptions.CircuitBreaker.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await using var circuitBreaker = httpClient.CircuitBreakerExecutor(resilienceOptions.CircuitBreaker, testHttpMessageHandler);
            var transientHttpStatusCode = HttpStatusCode.ServiceUnavailable;
            await circuitBreaker.TriggerFromTransientHttpStatusCodeAsync(transientHttpStatusCode);
            var response = await httpClient.GetAsync($"/circuit-breaker/transient-http-status-code/{transientHttpStatusCode}");
            response.ShouldNotBeOfType<CircuitBrokenHttpResponseMessage>(); // if circuit breaker policy is added a CircuitBrokenHttpResponseMessage is returned when the circuit is open
        }
    }
}
