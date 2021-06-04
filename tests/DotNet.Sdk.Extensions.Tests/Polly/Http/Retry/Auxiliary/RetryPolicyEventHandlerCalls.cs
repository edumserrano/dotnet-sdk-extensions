using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class RetryPolicyEventHandlerCalls
    {
        public IList<RetryEvent> OnRetryAsyncCalls { get; } = new List<RetryEvent>();

        public void AddOnRetryAsync(RetryEvent retryEvent)
        {
            OnRetryAsyncCalls.Add(retryEvent);
        }
    }
}