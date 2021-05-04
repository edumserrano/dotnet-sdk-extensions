using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    internal class DefaultFallbackPolicyEventHandler : IFallbackPolicyEventHandler
    {
        public Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            return Task.CompletedTask;
        }
    }
}