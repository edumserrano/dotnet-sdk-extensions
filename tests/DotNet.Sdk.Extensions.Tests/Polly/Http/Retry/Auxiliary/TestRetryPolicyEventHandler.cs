namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;

public class TestRetryPolicyEventHandler(RetryPolicyEventHandlerCalls retryPolicyEventHandlerCalls) : IRetryPolicyEventHandler
{
    private readonly RetryPolicyEventHandlerCalls _retryPolicyEventHandlerCalls = retryPolicyEventHandlerCalls;

    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        _retryPolicyEventHandlerCalls.AddOnRetry(retryEvent);
        return Task.CompletedTask;
    }
}
