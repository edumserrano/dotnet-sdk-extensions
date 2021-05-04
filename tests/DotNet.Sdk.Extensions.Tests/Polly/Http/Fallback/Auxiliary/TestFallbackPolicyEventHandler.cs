using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public class TestFallbackPolicyEventHandler : IFallbackPolicyEventHandler
    {
        public static IList<TimeoutFallbackEvent> OnTimeoutFallbackAsyncCalls { get; } = new List<TimeoutFallbackEvent>();

        public static IList<BrokenCircuitFallbackEvent> OnBrokenCircuitFallbackAsyncAsyncCalls { get; } = new List<BrokenCircuitFallbackEvent>();

        public static IList<TaskCancelledFallbackEvent> OnTaskCancelledFallbackAsyncCalls { get; } = new List<TaskCancelledFallbackEvent>();

        public Task OnTimeoutFallbackAsync(TimeoutFallbackEvent timeoutFallbackEvent)
        {
            OnTimeoutFallbackAsyncCalls.Add(timeoutFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(BrokenCircuitFallbackEvent brokenCircuitFallbackEvent)
        {
            OnBrokenCircuitFallbackAsyncAsyncCalls.Add(brokenCircuitFallbackEvent);
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(TaskCancelledFallbackEvent taskCancelledFallbackEvent)
        {
            OnTaskCancelledFallbackAsyncCalls.Add(taskCancelledFallbackEvent);
            return Task.CompletedTask;
        }
        
        public static void Clear()
        {
            OnTimeoutFallbackAsyncCalls.Clear();
            OnBrokenCircuitFallbackAsyncAsyncCalls.Clear();
            OnTaskCancelledFallbackAsyncCalls.Clear();
        }
    }
}
