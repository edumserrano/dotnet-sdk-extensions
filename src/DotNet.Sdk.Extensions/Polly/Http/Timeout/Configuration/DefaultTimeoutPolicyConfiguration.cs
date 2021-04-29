using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration
{
    internal class DefaultTimeoutPolicyConfiguration : ITimeoutPolicyConfiguration
    {
        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            return Task.CompletedTask;
        }
    }
}