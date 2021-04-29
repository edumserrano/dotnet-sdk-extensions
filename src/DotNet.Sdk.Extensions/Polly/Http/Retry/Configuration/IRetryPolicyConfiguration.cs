using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration
{
    public interface IRetryPolicyConfiguration
    {
        Task OnRetryAsync(RetryEvent retryEvent);
    }
}
