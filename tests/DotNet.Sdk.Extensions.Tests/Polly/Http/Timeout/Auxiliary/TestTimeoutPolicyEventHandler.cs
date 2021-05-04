using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TimeoutPolicyEventHandlerCalls
    {
        public IList<TimeoutEvent> OnTimeoutAsyncCalls { get; } = new List<TimeoutEvent>();

        public void AddOnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            OnTimeoutAsyncCalls.Add(timeoutEvent);
        }

        public void ShouldBeAsExpected(
            int count,
            string httpClientName,
            TimeoutOptions timeoutOptions)
        {
            OnTimeoutAsyncCalls.Count.ShouldBe(count);
            foreach (var onTimeoutAsyncCall in OnTimeoutAsyncCalls)
            {
                onTimeoutAsyncCall.HttpClientName.ShouldBe(httpClientName);
                onTimeoutAsyncCall.TimeoutOptions.TimeoutInSecs.ShouldBe(timeoutOptions.TimeoutInSecs);
            }
        }
    }

    public class TestTimeoutPolicyEventHandler : ITimeoutPolicyEventHandler
    {
        private readonly TimeoutPolicyEventHandlerCalls _timeoutPolicyEventHandlerCalls;

        public TestTimeoutPolicyEventHandler(TimeoutPolicyEventHandlerCalls timeoutPolicyEventHandlerCalls)
        {
            _timeoutPolicyEventHandlerCalls = timeoutPolicyEventHandlerCalls;
        }

        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            _timeoutPolicyEventHandlerCalls.AddOnTimeoutAsync(timeoutEvent);
            
            return Task.CompletedTask;
        }
    }
}
