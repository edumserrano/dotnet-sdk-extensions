namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events;

internal sealed class DefaultRetryPolicyEventHandler : IRetryPolicyEventHandler
{
    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        return Task.CompletedTask;
    }
}
