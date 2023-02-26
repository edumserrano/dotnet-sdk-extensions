namespace DotNet.Sdk.Extensions.Polly.Http.Fallback;

internal static class FallbackPolicyFactory
{
    public static AsyncPolicyWrap<HttpResponseMessage> CreateFallbackPolicy(
        string httpClientName,
        IFallbackPolicyEventHandler policyEventHandler)
    {
        var httpRequestExceptionFallback = CreateFallbackPolicyForHttpRequestException(httpClientName, policyEventHandler);
        var timeoutFallback = CreateFallbackPolicyForTimeouts(httpClientName, policyEventHandler);
        var brokenCircuitFallback = CreateFallbackPolicyForBrokenCircuit(httpClientName, policyEventHandler);
        var abortedFallback = CreateFallbackPolicyForAbortedRequests(httpClientName, policyEventHandler);
        return Policy.WrapAsync(
            httpRequestExceptionFallback,
            timeoutFallback,
            brokenCircuitFallback,
            abortedFallback);
    }

    private static AsyncFallbackPolicy<HttpResponseMessage> CreateFallbackPolicyForAbortedRequests(string httpClientName, IFallbackPolicyEventHandler policyEventHandler)
    {
        // handle TaskCanceledException thrown by HttpClient when it times out with a fallback response
        return Policy<HttpResponseMessage>
            .Handle<TaskCanceledException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, _, _) =>
                {
                    // on newer versions .NET still throws TaskCanceledException but the inner exception is of type System.TimeoutException.
                    // see https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-error-handling
                    var exception = delegateResult.Exception;
                    return exception switch
                    {
                        { InnerException: TimeoutException } => Task.FromResult<HttpResponseMessage>(new TimeoutHttpResponseMessage(exception)),
                        _ => Task.FromResult<HttpResponseMessage>(new AbortedHttpResponseMessage(exception))
                    };
                },
                onFallbackAsync: (outcome, context) =>
                {
                    var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                    return policyEventHandler.OnTaskCancelledFallbackAsync(fallbackEvent);
                });
    }

    private static AsyncFallbackPolicy<HttpResponseMessage> CreateFallbackPolicyForBrokenCircuit(string httpClientName, IFallbackPolicyEventHandler policyEventHandler)
    {
        // handle BrokenCircuitException thrown by a circuit breaker policy
        return Policy<HttpResponseMessage>
            .Handle<BrokenCircuitException>()
            .Or<IsolatedCircuitException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, _, _) =>
                {
                    var exception = delegateResult.Exception;
                    var response = exception switch
                    {
                        IsolatedCircuitException => new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Isolated, exception),
                        BrokenCircuitException => new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Open, exception),
                        _ => throw new InvalidOperationException($"Unexpected circuit breaker exception type: {delegateResult.Exception.GetType()}")
                    };
                    return Task.FromResult<HttpResponseMessage>(response);
                },
                onFallbackAsync: (outcome, context) =>
                {
                    var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                    return policyEventHandler.OnBrokenCircuitFallbackAsync(fallbackEvent);
                });
    }

    private static AsyncFallbackPolicy<HttpResponseMessage> CreateFallbackPolicyForTimeouts(string httpClientName, IFallbackPolicyEventHandler policyEventHandler)
    {
        // handle TimeoutRejectedException thrown by a timeout policy
        return Policy<HttpResponseMessage>
            .Handle<TimeoutRejectedException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, _, _) =>
                {
                    var response = new TimeoutHttpResponseMessage(delegateResult.Exception);
                    return Task.FromResult<HttpResponseMessage>(response);
                },
                onFallbackAsync: (outcome, context) =>
                {
                    var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                    return policyEventHandler.OnTimeoutFallbackAsync(fallbackEvent);
                });
    }

    private static AsyncFallbackPolicy<HttpResponseMessage> CreateFallbackPolicyForHttpRequestException(string httpClientName, IFallbackPolicyEventHandler policyEventHandler)
    {
        // handle HttpRequestException thrown by the HttpClient
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .FallbackAsync(
                fallbackAction: (delegateResult, _, _) =>
                {
                    var response = new ExceptionHttpResponseMessage(delegateResult.Exception);
                    return Task.FromResult<HttpResponseMessage>(response);
                },
                onFallbackAsync: (outcome, context) =>
                {
                    var fallbackEvent = new FallbackEvent(httpClientName, outcome, context);
                    return policyEventHandler.OnHttpRequestExceptionFallbackAsync(fallbackEvent);
                });
    }
}
