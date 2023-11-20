namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

public class TestCircuitBreakerPolicyEventHandler(CircuitBreakerPolicyEventHandlerCalls circuitBreakerPolicyEventHandlerCalls): ICircuitBreakerPolicyEventHandler
{
    private readonly CircuitBreakerPolicyEventHandlerCalls _circuitBreakerPolicyEventHandlerCalls = circuitBreakerPolicyEventHandlerCalls;

    public Task OnBreakAsync(BreakEvent breakEvent)
    {
        _circuitBreakerPolicyEventHandlerCalls.AddOnBreak(breakEvent);
        return Task.CompletedTask;
    }

    public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
    {
        _circuitBreakerPolicyEventHandlerCalls.AddOnHalfOpen(halfOpenEvent);
        return Task.CompletedTask;
    }

    public Task OnResetAsync(ResetEvent resetEvent)
    {
        _circuitBreakerPolicyEventHandlerCalls.AddOnReset(resetEvent);
        return Task.CompletedTask;
    }
}
