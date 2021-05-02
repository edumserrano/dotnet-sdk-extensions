using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events
{
    public interface ICircuitBreakerPolicyEventHandler
    {
        Task OnBreakAsync(BreakEvent breakEvent);

        Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent);

        Task OnResetAsync(ResetEvent resetEvent);
    }
}