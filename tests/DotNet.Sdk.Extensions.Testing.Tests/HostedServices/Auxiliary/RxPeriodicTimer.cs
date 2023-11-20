namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

internal sealed class RxPeriodicTimer(TimeSpan period, IScheduler scheduler)
{
    private readonly TimeSpan _period = period;
    private readonly IScheduler _scheduler = scheduler;

    public Task WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        return Observable
            .Timer(_period, _scheduler)
            .ToTask(cancellationToken);
    }
}
