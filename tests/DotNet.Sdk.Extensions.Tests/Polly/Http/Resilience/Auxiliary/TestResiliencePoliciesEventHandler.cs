namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;

public class TestResiliencePoliciesEventHandler : IResiliencePoliciesEventHandler
{
    private readonly ResiliencePoliciesEventHandlerCalls _resiliencePoliciesEventHandlerCalls;

    public TestResiliencePoliciesEventHandler(ResiliencePoliciesEventHandlerCalls resiliencePoliciesEventHandlerCalls)
    {
        _resiliencePoliciesEventHandlerCalls = resiliencePoliciesEventHandlerCalls;
    }

    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Timeout.AddOnTimeout(timeoutEvent);
        return Task.CompletedTask;
    }

    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Retry.AddOnRetry(retryEvent);
        return Task.CompletedTask;
    }

    public Task OnBreakAsync(BreakEvent breakEvent)
    {
        _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnBreak(breakEvent);
        return Task.CompletedTask;
    }

    public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
    {
        _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnHalfOpen(halfOpenEvent);
        return Task.CompletedTask;
    }

    public Task OnResetAsync(ResetEvent resetEvent)
    {
        _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnReset(resetEvent);
        return Task.CompletedTask;
    }

    public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Fallback.AddOnHttpRequestExceptionFallback(fallbackEvent);
        return Task.CompletedTask;
    }

    public Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Fallback.AddOnTimeoutFallback(fallbackEvent);
        return Task.CompletedTask;
    }

    public Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Fallback.AddOnBrokenCircuitFallback(fallbackEvent);
        return Task.CompletedTask;
    }

    public Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent)
    {
        _resiliencePoliciesEventHandlerCalls.Fallback.AddOnTaskCancelledFallback(fallbackEvent);
        return Task.CompletedTask;
    }
}
