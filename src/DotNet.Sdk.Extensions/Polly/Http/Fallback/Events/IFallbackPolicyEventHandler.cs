using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    public interface IFallbackPolicyEventHandler
    {
        Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent);
        
        Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent);
        
        Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent);
    }
}