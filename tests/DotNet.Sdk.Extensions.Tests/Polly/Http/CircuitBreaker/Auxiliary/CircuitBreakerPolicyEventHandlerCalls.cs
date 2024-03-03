namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

public class CircuitBreakerPolicyEventHandlerCalls
{
    public IList<BreakEvent> OnBreakAsyncCalls { get; } = [];

    public IList<HalfOpenEvent> OnHalfOpenAsyncCalls { get; } = [];

    public IList<ResetEvent> OnResetAsyncCalls { get; } = [];

    public void AddOnBreak(BreakEvent breakEvent)
    {
        OnBreakAsyncCalls.Add(breakEvent);
    }

    public void AddOnHalfOpen(HalfOpenEvent halfOpenEvent)
    {
        OnHalfOpenAsyncCalls.Add(halfOpenEvent);
    }

    public void AddOnReset(ResetEvent resetEvent)
    {
        OnResetAsyncCalls.Add(resetEvent);
    }
}
