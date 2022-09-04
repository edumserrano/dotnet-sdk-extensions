namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions;

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
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
        using var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var resilienceOptions = new ResilienceOptions
        {
            Timeout = new TimeoutOptions
            {
                TimeoutInSecs = 0.25,
            },
            Retry = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01,
            },
            CircuitBreaker = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 5,
                SamplingDurationInSecs = 10,
                FailureThreshold = 0.6,
                MinimumThroughput = 10,
            },
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
            .Count
            .ShouldBe(1);
        // all requests fail and each request will do actually 3 requests because of the retry count is set to 2
        // after the minimum throughput for the circuit breaker is hit at 10 requests, the requests aren't even
        // retried, everything fails fast. When the circuit breaker state transitions to open there was
        // 7 retries
        // 1 req + 2 retries = 3 total requests on the circuit breaker failing
        // plus 1 req + 2 retries = 6 total requests on the circuit breaker failing
        // plus 1 req + 2 retries = 9 total requests on the circuit breaker failing
        // plus 1 req = 10 requests failing on the circuit breaker (now the circuit breaker state is open)
        // plus 1 retry (1 of the 2 retries that should happen) = this retry fails fast because the circuit state is open
        // plus 0 retry = the second retry does NOT happen because the circuit breaker returned a CircuitBrokenHttpResponseMessage which is not retried
        // Summing all retries = 7
        resiliencePoliciesEventHandlerCalls.Retry.OnRetryAsyncCalls
            .Count
            .ShouldBe(7);
        // even though there are 13 total requests made once the circuit breaker is open the remaining
        // requests don't actually get made, they don't pass through the the circuit breaker
        numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldNotBe(totalRequestsCount);
        numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(resilienceOptions.CircuitBreaker.MinimumThroughput);
    }

    /// <summary>
    /// Tests that when requests fail because of a timeout:
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
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
        using var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var resilienceOptions = new ResilienceOptions
        {
            Timeout = new TimeoutOptions
            {
                TimeoutInSecs = 0.25,
            },
            Retry = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01,
            },
            CircuitBreaker = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 5,
                SamplingDurationInSecs = 10,
                FailureThreshold = 0.6,
                MinimumThroughput = 10,
            },
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
        const string triggerTimeoutPath = "/timeout";
        var timeout = TimeSpan.FromSeconds(resilienceOptions.Timeout.TimeoutInSecs + 2);
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
            .Count
            .ShouldBe(1);
        // all requests fail and each request will do actually 3 requests because of the retry count is set to 2
        // after the minimum throughput for the circuit breaker is hit at 10 requests, the requests aren't even
        // retried, everything fails fast. When the circuit breaker state transitions to open there was
        // 7 retries
        // 1 req + 2 retries = 3 total requests on the circuit breaker failing
        // plus 1 req + 2 retries = 6 total requests on the circuit breaker failing
        // plus 1 req + 2 retries = 9 total requests on the circuit breaker failing
        // plus 1 req = 10 requests failing on the circuit breaker (now the circuit breaker state is open)
        // plus 1 retry (1 of the 2 retries that should happen) = this retry fails fast because the circuit state is open
        // plus 0 retry = the second retry does NOT happen because the circuit breaker returned a CircuitBrokenHttpResponseMessage which is not retried
        // Summing all retries = 7
        resiliencePoliciesEventHandlerCalls.Retry.OnRetryAsyncCalls
            .Count
            .ShouldBe(7);
        // even though there are 13 total requests made once the circuit breaker is open the remaining
        // requests don't actually get made, they don't pass through the the circuit breaker
        numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldNotBe(totalRequestsCount);
        numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(resilienceOptions.CircuitBreaker.MinimumThroughput);
    }
}
