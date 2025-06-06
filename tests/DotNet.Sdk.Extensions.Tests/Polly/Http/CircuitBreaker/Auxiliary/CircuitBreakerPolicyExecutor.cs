namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

/// <summary>
/// Indicates how the circuit breaker reset should be performed.
/// </summary>
public enum CircuitBreakerPolicyExecutorResetTypes
{
    /// <summary>
    /// The circuit breaker reset will be instant. This is done by invoking the
    /// <see cref="ICircuitBreakerPolicy.Reset"/> method.
    /// </summary>
    Quick,

    /// <summary>
    /// The circuit breaker reset will be done as normal. This is done by waiting
    /// until the circuit breaker transitions to half open and then to closed state.
    /// </summary>
    Normal,
}

public sealed class CircuitBreakerPolicyExecutor : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _resetRequestPath;
    private readonly CircuitBreakerOptions _circuitBreakerOptions;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;
    private readonly List<HttpStatusCode> _transientHttpStatusCodes;
    private CircuitBreakerPolicyExecutorResetTypes _resetType;

    public CircuitBreakerPolicyExecutor(
        HttpClient httpClient,
        CircuitBreakerOptions circuitBreakerOptions,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        _httpClient = httpClient;
        _circuitBreakerOptions = circuitBreakerOptions;
        _testHttpMessageHandler = testHttpMessageHandler;
        _resetRequestPath = HandleResetRequest();
        _transientHttpStatusCodes = [.. HttpStatusCodesExtensions.GetTransientHttpStatusCodes()];
        _resetType = CircuitBreakerPolicyExecutorResetTypes.Quick;
    }

    public CircuitBreakerPolicyExecutor WithReset(CircuitBreakerPolicyExecutorResetTypes resetType)
    {
        _resetType = resetType;
        return this;
    }

    public Task TriggerFromExceptionAsync<TException>(Exception exception)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);

        var requestPath = $"/circuit-breaker/exception/{exception.GetType().Name}";
        _testHttpMessageHandler.HandleException(requestPath, exception);

        return TriggerCircuitBreakerFromExceptionAsync<TException>(requestPath);
    }

    public Task TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode httpStatusCode)
    {
        var handledRequestPath = _testHttpMessageHandler.HandleTransientHttpStatusCode(
            requestPath: "/circuit-breaker/transient-http-status-code",
            responseHttpStatusCode: httpStatusCode);
        return TriggerCircuitBreakerFromTransientStatusCodeAsync(handledRequestPath, httpStatusCode);
    }

    private async Task WaitResetAsync()
    {
        // wait for the duration of break so that the circuit goes into half open state
        await Task.Delay(TimeSpan.FromSeconds(_circuitBreakerOptions.DurationOfBreakInSecs + 0.07));
        // successful response will move the circuit breaker into closed state
        var response = await _httpClient.GetAsync(_resetRequestPath);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Unexpected status code from closed circuit. Got {response.StatusCode} but expected {nameof(HttpStatusCode.OK)}.");
        }

        // make sure we transition to a new sampling window or else requests would still fall
        // in the previous sampling window where the circuit state had already been open and closed.
        await Task.Delay(TimeSpan.FromSeconds(_circuitBreakerOptions.SamplingDurationInSecs + 0.05));
    }

    public async Task ShouldBeOpenAsync(string requestPath)
    {
        var response = await _httpClient.GetAsync(requestPath);
        if (response is not CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage)
        {
            throw new InvalidOperationException($"The circuit should be open but wasn't. Unexpected response type from open circuit. Expected a {typeof(CircuitBrokenHttpResponseMessage)} but got a {response.GetType()} from requestPath: {requestPath}");
        }

        if (circuitBrokenHttpResponseMessage.StatusCode != HttpStatusCode.InternalServerError)
        {
            throw new InvalidOperationException($"Unexpected status code from open circuit. Got {response.StatusCode} but expected {nameof(HttpStatusCode.InternalServerError)} from requestPath: {requestPath}");
        }
    }

    private string HandleResetRequest()
    {
        const string handledRequestPath = "/circuit-breaker/reset";
        _testHttpMessageHandler.MockHttpResponse(builder =>
        {
            builder
                .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(handledRequestPath, StringComparison.OrdinalIgnoreCase))
                .RespondWith(() => new HttpResponseMessage(HttpStatusCode.OK));
        });
        return handledRequestPath;
    }

    private async Task TriggerCircuitBreakerFromTransientStatusCodeAsync(string requestPath, HttpStatusCode httpStatusCode)
    {
        if (!_transientHttpStatusCodes.Contains(httpStatusCode))
        {
            throw new ArgumentException($"{httpStatusCode} is not a transient HTTP status code.", nameof(httpStatusCode));
        }

        for (var i = 0; i < _circuitBreakerOptions.MinimumThroughput; i++)
        {
            var response = await _httpClient.GetAsync(requestPath);
            // the circuit should be closed during this loop which means it will be returning the
            // expected status code. Once the circuit is open it starts failing fast by returning
            // a CircuitBrokenHttpResponseMessage instance whose status code is 500
            if (response.StatusCode != httpStatusCode)
            {
                throw new InvalidOperationException($"Unexpected status code from closed circuit. Got {response.StatusCode} but expected {httpStatusCode}. Iteration {i} of minimum throughput {_circuitBreakerOptions.MinimumThroughput}");
            }
        }
    }

    private async Task TriggerCircuitBreakerFromExceptionAsync<TException>(string requestPath)
        where TException : Exception
    {
        for (var i = 0; i < _circuitBreakerOptions.MinimumThroughput; i++)
        {
            try
            {
                await _httpClient.GetAsync(requestPath);
            }
            catch (TException)
            {
                // avoids the exception being propagated in order to open the circuit once
                // the CircuitBreakerOptions.MinimumThroughput number of requests is reached
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        switch (_resetType)
        {
            case CircuitBreakerPolicyExecutorResetTypes.Quick:
                _httpClient.ResetCircuitBreakerPolicy();
                break;
            case CircuitBreakerPolicyExecutorResetTypes.Normal:
                await WaitResetAsync();
                break;
            default:
                throw new InvalidOperationException($"Failed to dispose {nameof(CircuitBreakerPolicyExecutor)} because of unknown {nameof(CircuitBreakerPolicyExecutorResetTypes)}: {_resetType}. Can only handle {nameof(CircuitBreakerPolicyExecutorResetTypes.Normal)} and {nameof(CircuitBreakerPolicyExecutorResetTypes.Quick)}");
        }
    }
}
