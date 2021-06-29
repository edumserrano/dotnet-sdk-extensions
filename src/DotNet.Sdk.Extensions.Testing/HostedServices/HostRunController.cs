using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    internal class HostRunController
    {
        private readonly RunUntilOptions _options;

        public HostRunController(RunUntilOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<RunUntilResult> RunUntilAsync(RunUntilPredicateAsync predicateAsync)
        {
            if (predicateAsync is null) throw new ArgumentNullException(nameof(predicateAsync));

            var runUntilResult = await Task.Run(async () =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(_options.Timeout);
                    do
                    {
                        // before checking the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                        await Task.Delay(_options.PredicateCheckInterval, cts.Token);
                    } while (!await predicateAsync());

                    return RunUntilResult.PredicateReturnedTrue;
                }
                catch (TaskCanceledException)
                {
                    return RunUntilResult.TimedOut;
                }
            });
            return runUntilResult;
        }

        public async Task<RunUntilResult> RunUntilAsync2(RunUntilPredicateAsync predicateAsync)
        {
            if (predicateAsync is null) throw new ArgumentNullException(nameof(predicateAsync));

            var runUntilResult = await Task.Run(async () =>
            {
                try
                {
                    var cts = new CancellationTokenSource(_options.Timeout);
                    do
                    {
                        // before checking the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                        await Task.Delay(_options.PredicateCheckInterval, cts.Token);
                    } while (!await predicateAsync());

                    return RunUntilResult.PredicateReturnedTrue;
                }
                catch (TaskCanceledException)
                {
                    return RunUntilResult.TimedOut;
                }
            });
            return runUntilResult;
        }
    }
}