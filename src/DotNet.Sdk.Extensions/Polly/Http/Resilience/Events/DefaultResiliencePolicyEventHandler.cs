using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Events
{
    public class DefaultResiliencePolicyEventHandler : IResiliencePolicyEventHandler
    {
        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            return Task.CompletedTask;
        }

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
