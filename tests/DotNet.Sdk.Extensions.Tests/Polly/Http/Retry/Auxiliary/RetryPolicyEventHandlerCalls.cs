namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;

public class RetryPolicyEventHandlerCalls
{
    public IList<RetryEvent> OnRetryAsyncCalls { get; } = new List<RetryEvent>();

    public void AddOnRetry(RetryEvent retryEvent)
    {
        OnRetryAsyncCalls.Add(retryEvent);
    }
}
