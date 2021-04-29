using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class TestRetryPolicyConfiguration : IRetryPolicyConfiguration
    {
        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            return Task.CompletedTask;
        }
    }
}
