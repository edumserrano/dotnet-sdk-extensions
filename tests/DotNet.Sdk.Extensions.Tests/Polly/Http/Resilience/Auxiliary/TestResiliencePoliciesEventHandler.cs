using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    public class TestResiliencePoliciesEventHandler : IResiliencePoliciesEventHandler
    {
        private readonly TestFallbackPolicyEventHandler _fallbackPolicyEventHandler;

        public TestResiliencePoliciesEventHandler()
        {
            _fallbackPolicyEventHandler = new TestFallbackPolicyEventHandler();
        }

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
            return _fallbackPolicyEventHandler.OnTimeoutFallbackAsync(timeoutFallbackEvent);
        }

        public Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            return _fallbackPolicyEventHandler.OnBrokenCircuitFallbackAsync(brokenCircuitFallbackEvent);
        }

        public Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            return _fallbackPolicyEventHandler.OnTaskCancelledFallbackAsync(taskCancelledFallbackEvent);
        }

        public static void Clear()
        {
            TestFallbackPolicyEventHandler.Clear();
        }
    }
}
