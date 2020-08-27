using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        /// <summary>
        /// Terminates the host after the specified timeout.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
        /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to terminate after the timeout.</param>
        /// <param name="timeout">Timeout value.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilTimeoutAsync<T>(this WebApplicationFactory<T> webApplicationFactory, TimeSpan timeout) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));

            RunUntilPredicateAsync noOpPredicateAsync = () => Task.FromResult(false);
            var options = new RunUntilOptions { Timeout = timeout };
            return webApplicationFactory.RunUntilAsync(noOpPredicateAsync, options, throwExceptionIfTimeout: false, runUntilCancellationToken: default);
        }

        /*
        * When RunUntilAsync method is awaited the webserver will stay running until:
        * - the webserver app exits (ie: crashes or graceful shutdown)
        * - the predicate returns true and the server is disposed (non graceful shutdown)
        * - the timeout on the RunUntilOptions expires and the the server is disposed (non graceful shutdown)
        */
        internal static async Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            RunUntilPredicateAsync predicateAsyncAsync,
            RunUntilOptions options,
            bool throwExceptionIfTimeout,
            CancellationToken runUntilCancellationToken) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _ = webApplicationFactory.Server; //starts the server
            var hostRunController = new HostRunController(options);
            var runUntilResult = await hostRunController.RunUntil(predicateAsyncAsync, runUntilCancellationToken);
            webApplicationFactory.Dispose(); //shutdown the server (non graceful)
            if (runUntilResult == RunUntilResult.TimedOut && throwExceptionIfTimeout)
            {
                throw new RunUntilException($"{nameof(RunUntilWebApplicationFactoryExtensions)}.{nameof(RunUntilAsync)} timed out after {options.Timeout}. This usually means the test server was shutdown before the {nameof(RunUntilWebApplicationFactoryExtensions)}.{nameof(RunUntilAsync)} predicate returned true. If you want to run the server for a period of time consider using {nameof(RunUntilWebApplicationFactoryExtensions)}.{nameof(RunUntilTimeoutAsync)} instead");
            }
        }
    }
}