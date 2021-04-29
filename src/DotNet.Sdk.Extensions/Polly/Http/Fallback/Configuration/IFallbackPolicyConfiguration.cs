using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration
{
    public interface IFallbackPolicyConfiguration
    {
        Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent);
        
        Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent);
        
        Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent);
    }
}