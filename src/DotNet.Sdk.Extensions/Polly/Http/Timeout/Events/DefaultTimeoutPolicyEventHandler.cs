namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

internal sealed class DefaultTimeoutPolicyEventHandler : ITimeoutPolicyEventHandler
{
    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        return Task.CompletedTask;
    }
}
