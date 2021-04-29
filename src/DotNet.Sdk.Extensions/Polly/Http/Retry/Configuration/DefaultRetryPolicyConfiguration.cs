using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration
{
    internal class DefaultRetryPolicyConfiguration : IRetryPolicyConfiguration
    {
        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            return Task.CompletedTask;
        }
    }
}