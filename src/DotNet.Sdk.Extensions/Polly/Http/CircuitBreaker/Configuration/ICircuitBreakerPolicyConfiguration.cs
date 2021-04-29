using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration
{
    public interface ICircuitBreakerPolicyConfiguration
    {
        Task OnBreakAsync(BreakEvent breakEvent);

        Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent);

        Task OnResetAsync(ResetEvent resetEvent);
    }
}