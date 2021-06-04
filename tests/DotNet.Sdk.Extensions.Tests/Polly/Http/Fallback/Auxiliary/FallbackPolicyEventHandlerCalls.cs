using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public class FallbackPolicyEventHandlerCalls
    {
        public IList<FallbackEvent> OnHttpRequestExceptionFallbackAsyncCalls { get; } = new List<FallbackEvent>();

        public IList<FallbackEvent> OnTimeoutFallbackAsyncCalls { get; } = new List<FallbackEvent>();
        
        public IList<FallbackEvent> OnBrokenCircuitFallbackAsyncCalls { get; } = new List<FallbackEvent>();
        
        public IList<FallbackEvent> OnTaskCancelledFallbackAsyncCalls { get; } = new List<FallbackEvent>();
        
        public void AddOnHttpRequestExceptionFallback(FallbackEvent fallbackEvent)
        {
            OnHttpRequestExceptionFallbackAsyncCalls.Add(fallbackEvent);
        }

        public void AddOnTimeoutFallback(FallbackEvent fallbackEvent)
        {
            OnTimeoutFallbackAsyncCalls.Add(fallbackEvent);
        }

        public void AddOnBrokenCircuitFallback(FallbackEvent fallbackEvent)
        {
            OnBrokenCircuitFallbackAsyncCalls.Add(fallbackEvent);
        }

        public void AddOnTaskCancelledFallback(FallbackEvent fallbackEvent)
        {
            OnTaskCancelledFallbackAsyncCalls.Add(fallbackEvent);
        }
    }
}