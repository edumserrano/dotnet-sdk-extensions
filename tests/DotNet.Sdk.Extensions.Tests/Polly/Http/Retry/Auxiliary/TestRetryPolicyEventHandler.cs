using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class TestRetryPolicyEventHandler : IRetryPolicyEventHandler
    {
        private readonly RetryPolicyEventHandlerCalls _retryPolicyEventHandlerCalls;

        public TestRetryPolicyEventHandler(RetryPolicyEventHandlerCalls retryPolicyEventHandlerCalls)
        {
            _retryPolicyEventHandlerCalls = retryPolicyEventHandlerCalls;
        }

        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            _retryPolicyEventHandlerCalls.AddOnRetryAsync(retryEvent);
            return Task.CompletedTask;
        }
    }
}
