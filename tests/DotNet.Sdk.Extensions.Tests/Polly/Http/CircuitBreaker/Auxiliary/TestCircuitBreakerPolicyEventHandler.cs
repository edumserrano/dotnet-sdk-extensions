using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public class TestCircuitBreakerPolicyEventHandler : ICircuitBreakerPolicyEventHandler
    {
        public static IList<BreakEvent> OnBreakAsyncCalls { get; } = new List<BreakEvent>();
        
        public static IList<HalfOpenEvent> OnHalfOpenAsyncCalls { get; } = new List<HalfOpenEvent>();
        
        public static IList<ResetEvent> OnResetAsyncCalls { get; } = new List<ResetEvent>();


        public Task OnBreakAsync(BreakEvent breakEvent)
        {
            OnBreakAsyncCalls.Add(breakEvent);
            return Task.CompletedTask;
        }

        public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
        {
            OnHalfOpenAsyncCalls.Add(halfOpenEvent);
            return Task.CompletedTask;
        }

        public Task OnResetAsync(ResetEvent resetEvent)
        {
            OnResetAsyncCalls.Add(resetEvent);
            return Task.CompletedTask;
        }
    }
}
