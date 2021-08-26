using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TimeoutPolicyEventHandlerCalls
    {
        public IList<TimeoutEvent> OnTimeoutAsyncCalls { get; } = new List<TimeoutEvent>();

        public void AddOnTimeout(TimeoutEvent timeoutEvent)
        {
            OnTimeoutAsyncCalls.Add(timeoutEvent);
        }
    }
}
