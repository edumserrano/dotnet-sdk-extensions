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
        private readonly TestCircuitBreakerPolicyEventHandler _circuitBreakerPolicyEventHandler;
        private readonly TestFallbackPolicyEventHandler _fallbackPolicyEventHandler;

        public TestResiliencePoliciesEventHandler()
        {
            _circuitBreakerPolicyEventHandler = new TestCircuitBreakerPolicyEventHandler();
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
            return _circuitBreakerPolicyEventHandler.OnBreakAsync(breakEvent);
        }

        public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
        {
            return _circuitBreakerPolicyEventHandler.OnHalfOpenAsync(halfOpenEvent);
        }

        public Task OnResetAsync(ResetEvent resetEvent)
        {
            return _circuitBreakerPolicyEventHandler.OnResetAsync(resetEvent);
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
            TestCircuitBreakerPolicyEventHandler.Clear();
            TestFallbackPolicyEventHandler.Clear();
        }
    }
}
