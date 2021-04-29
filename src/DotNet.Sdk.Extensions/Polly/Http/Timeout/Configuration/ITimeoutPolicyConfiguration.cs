using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration
{
    public interface ITimeoutPolicyConfiguration
    {
        Task OnTimeoutAsync(TimeoutEvent timeoutEvent);
    }
}