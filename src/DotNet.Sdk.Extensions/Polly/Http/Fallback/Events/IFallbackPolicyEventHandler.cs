using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    public interface IFallbackPolicyEventHandler
    {
        Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent);

        Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent);
        
        Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent);
        
        Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent);
    }
}