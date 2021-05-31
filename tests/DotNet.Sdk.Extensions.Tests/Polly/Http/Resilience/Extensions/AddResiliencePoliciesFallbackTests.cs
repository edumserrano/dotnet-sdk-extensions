using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to test that the fallback policy is added.
    ///
    /// There should be more tests for the conditions that the fallback policy handles. For instance, tests
    /// for when a <see cref="BrokenCircuitException"/> happens the fallback is a <see cref="CircuitBrokenHttpResponseMessage"/>.
    /// However, when all the policies are added together via the AddResiliencePolicies extension method,
    /// it is quite hard to be able to test all of the fallback conditions.
    /// Not to worry much since the AddResiliencePolicies reuses the AddFallbackPolicy which is thoroughly tested
    /// by <see cref="AddFallbackPolicyTests"/>.
    /// 
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesFallbackTests
    {
        /// <summary>
        /// Tests that when requests fail because of a timeout but the fallback policy is disabled
        /// then you do not get a TimeoutHttpResponseMessage, you get a TimeoutRejectedException.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy1()
        {
            
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
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
                    DurationOfBreakInSecs = 5,
                    SamplingDurationInSecs = 10,
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
            var triggerTimeoutPath = "/timeout";
            var timeout = TimeSpan.FromSeconds(resilienceOptions.Timeout.TimeoutInSecs + 1);
            testHttpMessageHandler.HandleTimeout(triggerTimeoutPath, timeout);

            await Should.ThrowAsync<TimeoutRejectedException>(() => httpClient.GetAsync(triggerTimeoutPath));
        }

        /// <summary>
        /// Tests that when requests fail because of a timeout but the fallback policy is enabled
        /// then you do not get a TimeoutRejectedException you get a TimeoutHttpResponseMessage.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = true,
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
                    DurationOfBreakInSecs = 5,
                    SamplingDurationInSecs = 10,
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
            var triggerTimeoutPath = "/timeout";
            var timeout = TimeSpan.FromSeconds(resilienceOptions.Timeout.TimeoutInSecs + 1);
            testHttpMessageHandler.HandleTimeout(triggerTimeoutPath, timeout);

            var response = await httpClient.GetAsync(triggerTimeoutPath);
            response.ShouldBeOfType<TimeoutHttpResponseMessage>();
        }
    }
}
