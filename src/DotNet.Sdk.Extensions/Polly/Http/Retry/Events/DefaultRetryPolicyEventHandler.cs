using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events
{
    internal class DefaultRetryPolicyEventHandler : IRetryPolicyEventHandler
    {
        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            return Task.CompletedTask;
        }
    }
}