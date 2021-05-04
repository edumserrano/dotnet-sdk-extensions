using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events
{
    internal class DefaultCircuitBreakerPolicyEventHandler : ICircuitBreakerPolicyEventHandler
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