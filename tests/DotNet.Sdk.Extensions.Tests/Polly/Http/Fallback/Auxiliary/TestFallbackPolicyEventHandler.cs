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

        public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnHttpRequestExceptionFallback(fallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnTimeoutFallback(fallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnBrokenCircuitFallback(fallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent)
        {
            _fallbackPolicyEventHandlerCalls.AddOnTaskCancelledFallback(fallbackEvent);
            return Task.CompletedTask;
        }
    }
}
