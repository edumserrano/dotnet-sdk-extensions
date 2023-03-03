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
        if (predicateAsync is null)
        {
            throw new ArgumentNullException(nameof(predicateAsync));
        }


        var a = Observable
            .Interval(_options.PredicateCheckInterval)
            .ToAsyncEnumerable();

        //Observable
        //    .Interval(_options.PredicateCheckInterval, _scheduler)
        //    .Subscribe(_ =>
        //    {
        //        await predicateAsync()
        //    }, stoppingToken);
        //try
        //{
        //    await Task.Delay(Timeout.Infinite, stoppingToken);
        //}
        //catch (OperationCanceledException)
        //{
        //    // ignore, do nothing if when the Task.Delay throws exception because the host is being terminated
        //}

#if NET6_0 || NET7_0
        try
        {
            using var cts = new CancellationTokenSource(_options.Timeout);
            using var timer = new PeriodicTimer(_options.PredicateCheckInterval);
            do
            {
                // before checking the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                await timer.WaitForNextTickAsync(cts.Token);
            }
            while (!await predicateAsync());

            return RunUntilResult.PredicateReturnedTrue;
        }
        catch (OperationCanceledException)
        {
            return RunUntilResult.TimedOut;
        }
#else
        try
        {
            using var cts = new CancellationTokenSource(_options.Timeout);
            do
            {
                // before checking the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                await Task.Delay(_options.PredicateCheckInterval, cts.Token);
            }
            while (!await predicateAsync());

            return RunUntilResult.PredicateReturnedTrue;
        }
        catch (TaskCanceledException)
        {
            return RunUntilResult.TimedOut;
        }
#endif
    }
}
