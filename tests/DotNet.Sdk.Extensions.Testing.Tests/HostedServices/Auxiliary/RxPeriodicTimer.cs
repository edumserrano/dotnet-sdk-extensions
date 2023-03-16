namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

internal sealed class RxPeriodicTimer
{
    private readonly TimeSpan _period;
    private readonly IScheduler _scheduler;

    public RxPeriodicTimer(TimeSpan period, IScheduler scheduler)
    {
        _period = period;
        _scheduler = scheduler;
    }

    public Task WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        return Observable
            .Timer(_period, _scheduler)
            .ToTask(cancellationToken);
    }
}
