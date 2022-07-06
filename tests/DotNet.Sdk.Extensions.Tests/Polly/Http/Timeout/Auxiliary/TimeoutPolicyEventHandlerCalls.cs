namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

public class TimeoutPolicyEventHandlerCalls
{
    public IList<TimeoutEvent> OnTimeoutAsyncCalls { get; } = new List<TimeoutEvent>();

    public void AddOnTimeout(TimeoutEvent timeoutEvent)
    {
        OnTimeoutAsyncCalls.Add(timeoutEvent);
    }
}
