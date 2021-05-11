using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public class FallbackPolicyEventHandlerCalls
    {
        public IList<TimeoutFallbackEvent> OnTimeoutFallbackAsyncCalls { get; } = new List<TimeoutFallbackEvent>();
        
        public IList<BrokenCircuitFallbackEvent> OnBrokenCircuitFallbackAsyncCalls { get; } = new List<BrokenCircuitFallbackEvent>();
        
        public IList<TaskCancelledFallbackEvent> OnTaskCancelledFallbackAsyncCalls { get; } = new List<TaskCancelledFallbackEvent>();
        
        public void AddOnTimeoutFallback(TimeoutFallbackEvent timeoutFallbackEvent)
        {
            OnTimeoutFallbackAsyncCalls.Add(timeoutFallbackEvent);
        }

        public void AddOnBrokenCircuitFallback(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            OnBrokenCircuitFallbackAsyncCalls.Add(brokenCircuitFallbackEvent);
        }

        public void AddOnTaskCancelledFallback(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            OnTaskCancelledFallbackAsyncCalls.Add(taskCancelledFallbackEvent);
        }
    }
}