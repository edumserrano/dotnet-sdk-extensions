using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public class TestCircuitBreakerPolicyEventHandler : ICircuitBreakerPolicyEventHandler
    {
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
    }
}
