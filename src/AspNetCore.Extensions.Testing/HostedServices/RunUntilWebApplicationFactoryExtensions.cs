﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AspNetCore.Extensions.Testing.HostedServices
{
    /*
     * RunUntil WebApplicationFactory extension methods
     */
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        public static Task RunUntilTimeoutAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            TimeSpan timeout) where T : class
        {
            Func<Task<bool>> noOpPredicate = () => Task.FromResult(false);
            var options = new RunUntilOptions { Timeout = timeout };
            return webApplicationFactory.RunUntilAsync(noOpPredicate, options, throwExceptionIfTimeout: false, runUntilCancellationToken: default);
        }

        /*
        * When RunUntilAsync method is awaited the webserver will stay running until:
        * - the webserver app exits (ie: crashes or graceful shutdown)
        * - the predicate returns true and the server is disposed (non graceful shutdown)
        * - the timeout on the RunUntilOptions expires and the the server is disposed (non graceful shutdown)
        */
        internal static async Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            Func<Task<bool>> predicateAsync,
            RunUntilOptions options,
            bool throwExceptionIfTimeout,
            CancellationToken runUntilCancellationToken) where T : class
        {
            _ = webApplicationFactory.Server; //starts the server
            var hostRunController = new HostRunController(options);
            var runUntilResult = await hostRunController.RunUntil(predicateAsync, runUntilCancellationToken);
            webApplicationFactory.Dispose(); //shutdown the server (non graceful)
            if (runUntilResult == RunUntilResult.TimedOut && throwExceptionIfTimeout)
            {
                throw new RunUntilException("RunUntilAsync timed out. This usually means the test server was shutdown before the RunUntilAsync predicate returned true. If you want to run the server for a period of time consider using RunUntilTimeout instead");
            }
        }
    }
}