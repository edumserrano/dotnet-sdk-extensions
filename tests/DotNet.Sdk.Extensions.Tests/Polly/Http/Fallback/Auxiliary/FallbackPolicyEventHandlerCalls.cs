namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;

public class FallbackPolicyEventHandlerCalls
{
    public IList<FallbackEvent> OnHttpRequestExceptionFallbackAsyncCalls { get; } = [];

    public IList<FallbackEvent> OnTimeoutFallbackAsyncCalls { get; } = [];

    public IList<FallbackEvent> OnBrokenCircuitFallbackAsyncCalls { get; } = [];

    public IList<FallbackEvent> OnTaskCancelledFallbackAsyncCalls { get; } = [];

    public void AddOnHttpRequestExceptionFallback(FallbackEvent fallbackEvent)
    {
        OnHttpRequestExceptionFallbackAsyncCalls.Add(fallbackEvent);
    }

    public void AddOnTimeoutFallback(FallbackEvent fallbackEvent)
    {
        OnTimeoutFallbackAsyncCalls.Add(fallbackEvent);
    }

    public void AddOnBrokenCircuitFallback(FallbackEvent fallbackEvent)
    {
        OnBrokenCircuitFallbackAsyncCalls.Add(fallbackEvent);
    }

    public void AddOnTaskCancelledFallback(FallbackEvent fallbackEvent)
    {
        OnTaskCancelledFallbackAsyncCalls.Add(fallbackEvent);
    }
}
