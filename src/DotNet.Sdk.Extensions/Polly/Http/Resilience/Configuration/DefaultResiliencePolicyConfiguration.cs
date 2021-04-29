using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration
{
    public class DefaultResiliencePolicyConfiguration : IResiliencePolicyConfiguration
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
