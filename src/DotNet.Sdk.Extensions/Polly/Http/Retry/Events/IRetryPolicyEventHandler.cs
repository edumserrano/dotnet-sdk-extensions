using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events
{
    public interface IRetryPolicyEventHandler
    {
        Task OnRetryAsync(RetryEvent retryEvent);
    }
}
