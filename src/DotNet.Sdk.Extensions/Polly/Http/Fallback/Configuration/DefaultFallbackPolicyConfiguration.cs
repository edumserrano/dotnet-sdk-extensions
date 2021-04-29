using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration
{
    internal class DefaultFallbackPolicyConfiguration : IFallbackPolicyConfiguration
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