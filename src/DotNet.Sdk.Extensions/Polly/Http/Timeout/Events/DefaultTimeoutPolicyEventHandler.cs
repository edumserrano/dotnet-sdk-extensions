using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Events
{
    internal class DefaultTimeoutPolicyEventHandler : ITimeoutPolicyEventHandler
    {
        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            return Task.CompletedTask;
        }
    }
}
