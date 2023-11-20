namespace DotNet.Sdk.Extensions.Testing.HostedServices;

internal sealed class RxPeriodicTimer(
    TimeSpan period,
    TimeSpan timeout,
    IScheduler scheduler)
{
    private readonly TimeSpan _period = period;
    private readonly DateTimeOffset _timeoutTime = scheduler.Now.Add(timeout);
    private readonly IScheduler _scheduler = scheduler;

    public Task WaitForNextTickAsync()
    {
        return Observable
            .Timer(_period, _scheduler)
            .Timeout(_timeoutTime, _scheduler)
            .ToTask();
    }
}
