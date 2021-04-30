using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TestTimeoutPolicyEventReceiver : ITimeoutPolicyEventHandler
    {
        public static IList<TimeoutEvent> OnTimeoutAsyncCalls { get; } = new List<TimeoutEvent>();

        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            OnTimeoutAsyncCalls.Add(timeoutEvent);
            return Task.CompletedTask;
        }
    }
}
