using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Events
{
    public interface ITimeoutPolicyEventHandler
    {
        Task OnTimeoutAsync(TimeoutEvent timeoutEvent);
    }
}