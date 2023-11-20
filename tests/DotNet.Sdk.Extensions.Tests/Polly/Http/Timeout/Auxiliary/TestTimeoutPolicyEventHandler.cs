namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

public class TestTimeoutPolicyEventHandler(TimeoutPolicyEventHandlerCalls timeoutPolicyEventHandlerCalls): ITimeoutPolicyEventHandler
{
    private readonly TimeoutPolicyEventHandlerCalls _timeoutPolicyEventHandlerCalls = timeoutPolicyEventHandlerCalls;

    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        _timeoutPolicyEventHandlerCalls.AddOnTimeout(timeoutEvent);
        return Task.CompletedTask;
    }
}
