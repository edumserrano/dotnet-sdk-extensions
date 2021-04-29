using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TestTimeoutPolicyConfiguration : ITimeoutPolicyConfiguration
    {
        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            return Task.CompletedTask;
        }
    }
}
