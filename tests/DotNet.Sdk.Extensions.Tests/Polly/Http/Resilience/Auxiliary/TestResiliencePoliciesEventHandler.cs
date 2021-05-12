using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    public class TestResiliencePoliciesEventHandler : IResiliencePoliciesEventHandler
    {
        private readonly ResiliencePoliciesEventHandlerCalls _resiliencePoliciesEventHandlerCalls;

        public TestResiliencePoliciesEventHandler(ResiliencePoliciesEventHandlerCalls resiliencePoliciesEventHandlerCalls)
        {
            _resiliencePoliciesEventHandlerCalls = resiliencePoliciesEventHandlerCalls;
        }

        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            _resiliencePoliciesEventHandlerCalls.Timeout.AddOnTimeoutAsync(timeoutEvent);
            return Task.CompletedTask;
        }
       
        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            _resiliencePoliciesEventHandlerCalls.Retry.AddOnRetryAsync(retryEvent);
            return Task.CompletedTask;
        }

        public Task OnBreakAsync(BreakEvent breakEvent)
        {
            _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnBreakAsync(breakEvent);
            return Task.CompletedTask;
        }

        public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
        {
            _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnHalfOpenAsync(halfOpenEvent);
            return Task.CompletedTask;
        }

        public Task OnResetAsync(ResetEvent resetEvent)
        {
            _resiliencePoliciesEventHandlerCalls.CircuitBreaker.AddOnResetAsync(resetEvent);
            return Task.CompletedTask;
        }

        public Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent)
        {
            _resiliencePoliciesEventHandlerCalls.Fallback.AddOnTimeoutFallback(timeoutFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            _resiliencePoliciesEventHandlerCalls.Fallback.AddOnBrokenCircuitFallback(brokenCircuitFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            _resiliencePoliciesEventHandlerCalls.Fallback.AddOnTaskCancelledFallback(taskCancelledFallbackEvent);
            return Task.CompletedTask;
        }
    }
}
