using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public class TestCircuitBreakerPolicyEventHandler : ICircuitBreakerPolicyEventHandler
    {
        private readonly CircuitBreakerPolicyEventHandlerCalls _circuitBreakerPolicyEventHandlerCalls;

        public TestCircuitBreakerPolicyEventHandler(CircuitBreakerPolicyEventHandlerCalls circuitBreakerPolicyEventHandlerCalls)
        {
            _circuitBreakerPolicyEventHandlerCalls = circuitBreakerPolicyEventHandlerCalls;
        }

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
}
