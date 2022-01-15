using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TestTimeoutPolicyEventHandler : ITimeoutPolicyEventHandler
    {
        private readonly TimeoutPolicyEventHandlerCalls _timeoutPolicyEventHandlerCalls;

        public TestTimeoutPolicyEventHandler(TimeoutPolicyEventHandlerCalls timeoutPolicyEventHandlerCalls)
        {
            _timeoutPolicyEventHandlerCalls = timeoutPolicyEventHandlerCalls;
        }

        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            _timeoutPolicyEventHandlerCalls.AddOnTimeout(timeoutEvent);
            return Task.CompletedTask;
        }
    }
}
