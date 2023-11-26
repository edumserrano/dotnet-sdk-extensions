namespace DotNet.Sdk.Extensions.Testing.HostedServices;

internal sealed class HostRunController
{
    private readonly RunUntilOptions _options;
    private readonly IScheduler _scheduler;

    public HostRunController(RunUntilOptions options, IScheduler scheduler)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public async Task<RunUntilResult> RunUntilAsync(RunUntilPredicateAsync predicateAsync)
    {
        ArgumentNullException.ThrowIfNull(predicateAsync);

        try
        {
            var timer = new RxPeriodicTimer(
                _options.PredicateCheckInterval,
                _options.Timeout,
                _scheduler);
            do
            {
                // before checking the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                await timer.WaitForNextTickAsync();
            }
            while (!await predicateAsync());
            return RunUntilResult.PredicateReturnedTrue;
        }
        catch (TimeoutException)
        {
            return RunUntilResult.TimedOut;
        }
    }
}
