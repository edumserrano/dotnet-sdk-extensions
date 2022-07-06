namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;

internal class DefaultResiliencePoliciesEventHandler : IResiliencePoliciesEventHandler
{
    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnBreakAsync(BreakEvent breakEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnResetAsync(ResetEvent resetEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent)
    {
        return Task.CompletedTask;
    }

    public Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent)
    {
        return Task.CompletedTask;
    }
}
