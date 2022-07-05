using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

public class CircuitBreakerPolicyEventHandlerCalls
{
    public IList<BreakEvent> OnBreakAsyncCalls { get; } = new List<BreakEvent>();

    public IList<HalfOpenEvent> OnHalfOpenAsyncCalls { get; } = new List<HalfOpenEvent>();

    public IList<ResetEvent> OnResetAsyncCalls { get; } = new List<ResetEvent>();

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
