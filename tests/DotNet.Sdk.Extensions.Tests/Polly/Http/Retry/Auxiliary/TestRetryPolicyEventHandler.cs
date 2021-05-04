using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class TestRetryPolicyEventHandler : IRetryPolicyEventHandler
    {
        public static IList<RetryEvent> OnRetryAsyncCalls { get; } = new List<RetryEvent>();

        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            OnRetryAsyncCalls.Add(retryEvent);
            return Task.CompletedTask;
        }

        public static void Clear()
        {
            OnRetryAsyncCalls.Clear();
        }
    }
}
