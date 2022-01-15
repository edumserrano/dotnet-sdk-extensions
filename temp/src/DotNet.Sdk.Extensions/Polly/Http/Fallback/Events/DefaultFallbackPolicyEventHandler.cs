using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    internal class DefaultFallbackPolicyEventHandler : IFallbackPolicyEventHandler
    {
        public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent)
        {
            return Task.CompletedTask;
        }
    }
}
