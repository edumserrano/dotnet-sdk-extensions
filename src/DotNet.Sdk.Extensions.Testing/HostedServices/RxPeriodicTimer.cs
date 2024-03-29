namespace DotNet.Sdk.Extensions.Testing.HostedServices;

internal sealed class RxPeriodicTimer
{
    private readonly TimeSpan _period;
    private readonly DateTimeOffset _timeoutTime;
    private readonly IScheduler _scheduler;

    public RxPeriodicTimer(
        TimeSpan period,
        TimeSpan timeout,
        IScheduler scheduler)
    {
        _period = period;
        _timeoutTime = scheduler.Now.Add(timeout);
        _scheduler = scheduler;
    }

    public Task WaitForNextTickAsync()
    {
        return Observable
            .Timer(_period, _scheduler)
            .Timeout(_timeoutTime, _scheduler)
            .ToTask();
    }
}
