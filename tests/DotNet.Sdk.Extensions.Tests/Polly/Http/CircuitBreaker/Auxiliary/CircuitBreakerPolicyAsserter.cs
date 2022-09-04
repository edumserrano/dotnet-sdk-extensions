namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

internal static class CircuitBreakerPolicyAsserterExtensions
{
    public static CircuitBreakerPolicyAsserter CircuitBreakerPolicyAsserter(
        this HttpClient httpClient,
        CircuitBreakerOptions options,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new CircuitBreakerPolicyAsserter(httpClient, options, testHttpMessageHandler);
    }
}

internal class CircuitBreakerPolicyAsserter
{
    private readonly HttpClient _httpClient;
    private readonly CircuitBreakerOptions _options;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;
    private CircuitBreakerPolicyExecutorResetTypes _resetType;

    public CircuitBreakerPolicyAsserter(
        HttpClient httpClient,
        CircuitBreakerOptions options,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        _httpClient = httpClient;
        _options = options;
        _testHttpMessageHandler = testHttpMessageHandler;
        _resetType = CircuitBreakerPolicyExecutorResetTypes.Quick;
    }

    public CircuitBreakerPolicyAsserter WithReset(CircuitBreakerPolicyExecutorResetTypes resetType)
    {
        _resetType = resetType;
        return this;
    }

    public async Task HttpClientShouldContainCircuitBreakerPolicyAsync()
    {
        await CircuitBreakerPolicyHandlesTransientStatusCodes();
        await CircuitBreakerPolicyHandlesException<HttpRequestException>();
        await CircuitBreakerPolicyHandlesException<TimeoutRejectedException>();
        await CircuitBreakerPolicyHandlesException<TaskCanceledException>();
    }

    public void EventHandlerShouldReceiveExpectedEvents(
        int count,
        string httpClientName,
        CircuitBreakerPolicyEventHandlerCalls eventHandlerCalls)
    {
        eventHandlerCalls
            .OnBreakAsyncCalls
            .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
            .ShouldBe(count);
        eventHandlerCalls
            .OnResetAsyncCalls
            .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
            .ShouldBe(count);

        // when the circuit breaker is used the break and reset events are always triggered. However, when
        // the circuit breaker reset type is set to quick, the on half open events are never triggered.
        // So we only check for then when the reset type is CircuitBreakerPolicyExecutorResetTypes.Normal
        if (_resetType == CircuitBreakerPolicyExecutorResetTypes.Normal)
        {
            eventHandlerCalls
                .OnHalfOpenAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                    && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                    && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                    && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                    && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
                .ShouldBe(count);
        }
    }

    private async Task CircuitBreakerPolicyHandlesTransientStatusCodes()
    {
        foreach (var transientHttpStatusCode in HttpStatusCodesExtensions.GetTransientHttpStatusCodes())
        {
#pragma warning disable CA2000 // Dispose objects before losing scope - false warning, _httpClient should not be disposed here
            await using var circuitBreaker = _httpClient
                .CircuitBreakerExecutor(_options, _testHttpMessageHandler)
                .WithReset(_resetType);
#pragma warning restore CA2000 // Dispose objects before losing scope - false warning, _httpClient should not be disposed here
            await circuitBreaker.TriggerFromTransientHttpStatusCodeAsync(transientHttpStatusCode);
            await circuitBreaker.ShouldBeOpenAsync($"/circuit-breaker/transient-http-status-code/{transientHttpStatusCode}");
        }
    }

    private Task CircuitBreakerPolicyHandlesException<TException>()
        where TException : Exception
    {
        var exception = Activator.CreateInstance<TException>();
        return CircuitBreakerPolicyHandlesException(exception);
    }

    private async Task CircuitBreakerPolicyHandlesException<TException>(TException exception)
        where TException : Exception
    {
#pragma warning disable CA2000 // Dispose objects before losing scope - false warning, _httpClient should not be disposed here
        await using var circuitBreaker = _httpClient
            .CircuitBreakerExecutor(_options, _testHttpMessageHandler)
            .WithReset(_resetType);
#pragma warning restore CA2000 // Dispose objects before losing scope - false warning, _httpClient should not be disposed here
        await circuitBreaker.TriggerFromExceptionAsync<TException>(exception);
        await circuitBreaker.ShouldBeOpenAsync($"/circuit-breaker/exception/{exception.GetType().Name}");
    }
}
