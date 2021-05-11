using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public class TestFallbackPolicyEventHandler : IFallbackPolicyEventHandler
    {
        private readonly FallbackPolicyEventHandlerCalls _fallbackPolicyEventHandlerCalls;

        public TestFallbackPolicyEventHandler(FallbackPolicyEventHandlerCalls fallbackPolicyEventHandlerCalls)
        {
            _fallbackPolicyEventHandlerCalls = fallbackPolicyEventHandlerCalls;
        }

        public Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnTimeoutFallback(timeoutFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnBrokenCircuitFallback(brokenCircuitFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnTaskCancelledFallback(taskCancelledFallbackEvent);
            return Task.CompletedTask;
        }
    }
}
