using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
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
    /// Specifically to test that the timeout, retry, circuit breaker and fallback policies work
    /// together as expected.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesAllPoliciesTests
    {
        /// <summary>
        /// Tests that when requests fail with a transient error http status code:
        /// - the retry policy retries requests
        /// - until the circuit breaker opens the response returned is the HttpResponseMessage returned by the HttpClient
        /// - once the circuit breaker opens requests fail fast, even when retried
        /// - once the circuit breaker opens the response returned is a CircuitBrokenHttpResponseMessage because of the CircuitBreakerCheckerAsyncPolicy
        ///
        /// Be aware of the interaction between the retry policy and the circuit breaker policy in regards to
        /// how the retry count and median first retry delay interact with the circuit breaker's options.
        /// For instance, the circuit breaker might not get triggered as expected if the sampling duration
        /// is smaller then the median first retry delay.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAllPolicies1()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
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
            var triggerCircuitBreakerPath = testHttpMessageHandler.HandleTransientHttpStatusCode(
                requestPath: "/circuit-breaker/transient-http-status-code",
                responseHttpStatusCode: HttpStatusCode.ServiceUnavailable);
            var httpResponses = new List<HttpResponseMessage>();
            // make enough requests to trigger the circuit breaker, then make sure we make some more
            // to prove once the circuit breaker is open it fails fast
            var totalRequestsCount = resilienceOptions.CircuitBreaker.MinimumThroughput + 3;
            for (var i = 0; i < totalRequestsCount; i++)
            {
                // because there's a retry policy each request made here will actually
                // result in multiple requests through the circuit breaker policy
                // ie: with retry count of 2 each request is done here is actually 3 requests
                // going through the circuit breaker
                var response = await httpClient.GetAsync(triggerCircuitBreakerPath);
                httpResponses.Add(response);
            }

            // the responses returned should be of type HttpResponseMessage, because they are what
            // was returned by the http client up until the circuit becomes open, after which the 
            // responses should be of type CircuitBrokenHttpResponseMessage, because it's what the
            // circuit breaker wrapped policy is returning to fail fast when the circuit is open
            for (var i = 0; i < httpResponses.Count; i++)
            {
                var response = httpResponses[i];
                var maxIndexBeforeCircuitGetsOpen = resilienceOptions.CircuitBreaker.MinimumThroughput / (resilienceOptions.Retry.RetryCount + 1);
                if (i < maxIndexBeforeCircuitGetsOpen)
                {
                    response.ShouldBeOfType(typeof(HttpResponseMessage));
                }
                else
                {
                    response.ShouldBeOfType(typeof(CircuitBrokenHttpResponseMessage));
                }
            }

            // the circuit breaker is opened once
            resiliencePoliciesEventHandlerCalls.CircuitBreaker.OnBreakAsyncCalls
                .Count()
                .ShouldBe(1);
            // because all requests fail the retry policy retries them all
            resiliencePoliciesEventHandlerCalls.Retry.OnRetryAsyncCalls
                .Count()
                .ShouldBe(resilienceOptions.Retry.RetryCount * totalRequestsCount);
            // even though there are 13 total requests made once the circuit breaker is open the remaining
            // requests don't actually get made, they don't pass through the the circuit breaker
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldNotBe(totalRequestsCount);
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(resilienceOptions.CircuitBreaker.MinimumThroughput);
        }

        /// <summary>
        /// Tests that when requests fail with a transient error http status code:
        /// - the retry policy retries requests
        /// - until the circuit breaker opens the response returned is a TimeoutHttpResponseMessage because of the fallback policy
        /// - once the circuit breaker opens requests fail fast, even when retried (it never even gets to the timeout policy)
        /// - once the circuit breaker opens the response returned is a CircuitBrokenHttpResponseMessage because of the circuit breaker policy
        /// 
        /// Be aware of the interaction between the retry policy and the circuit breaker policy in regards to
        /// how the retry count and median first retry delay interact with the circuit breaker's options.
        /// For instance, the circuit breaker might not get triggered as expected if the sampling duration
        /// is smaller then the median first retry delay.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAllPolicies2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
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
            var httpResponses = new List<HttpResponseMessage>();
            // make enough requests to trigger the circuit breaker, then make sure we make some more
            // to prove once the circuit breaker is open it fails fast
            var totalRequestsCount = resilienceOptions.CircuitBreaker.MinimumThroughput + 3;
            for (var i = 0; i < totalRequestsCount; i++)
            {
                // because there's a retry policy each request made here will actually
                // result in multiple requests through the circuit breaker policy
                // ie: with retry count of 2 each request is done here is actually 3 requests
                // going through the circuit breaker
                var response = await httpClient.GetAsync(triggerTimeoutPath);
                httpResponses.Add(response);
            }

            // the responses returned should be of type HttpResponseMessage, because they are what
            // was returned by the http client up until the circuit becomes open, after which the 
            // responses should be of type CircuitBrokenHttpResponseMessage, because it's what the
            // circuit breaker wrapped policy is returning to fail fast when the circuit is open
            for (var i = 0; i < httpResponses.Count; i++)
            {
                var response = httpResponses[i];
                var maxIndexBeforeCircuitGetsOpen = resilienceOptions.CircuitBreaker.MinimumThroughput / (resilienceOptions.Retry.RetryCount + 1);
                if (i < maxIndexBeforeCircuitGetsOpen)
                {
                    response.ShouldBeOfType(typeof(TimeoutHttpResponseMessage));
                }
                else
                {
                    response.ShouldBeOfType(typeof(CircuitBrokenHttpResponseMessage));
                }
            }

            // the circuit breaker is opened once
            resiliencePoliciesEventHandlerCalls.CircuitBreaker.OnBreakAsyncCalls
                .Count()
                .ShouldBe(1);
            // because all requests fail the retry policy retries them all
            resiliencePoliciesEventHandlerCalls.Retry.OnRetryAsyncCalls
                .Count()
                .ShouldBe(resilienceOptions.Retry.RetryCount * totalRequestsCount);
            // even though there are 13 total requests made once the circuit breaker is open the remaining
            // requests don't actually get made, they don't pass through the the circuit breaker
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldNotBe(totalRequestsCount);
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(resilienceOptions.CircuitBreaker.MinimumThroughput);
        }
    }
}
