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

        public async Task<RunUntilResult> RunUntil(RunUntilPredicateAsync predicateAsyncAsync, CancellationToken hostCancellationToken)
        {
            if (predicateAsyncAsync == null) throw new ArgumentNullException(nameof(predicateAsyncAsync));

            var runUntilResult = await Task.Run(async () =>
            {
                try
                {
                    var cts = new CancellationTokenSource(_options.Timeout);
                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, hostCancellationToken);
                    while (!await predicateAsyncAsync())
                    {
                        // before checking again the predicate, wait RunUntilOptions.PredicateLoopPeriod or abort if the RunUntilOptions.Timeout elapses
                        await Task.Delay(_options.PredicateCheckInterval, linkedCts.Token);
                    }
                    return RunUntilResult.PredicateReturnedTrue;
                }
                catch (TaskCanceledException)
                {
                    return RunUntilResult.TimedOut;
                }
            }, hostCancellationToken);
            return runUntilResult;
        }
    }
}