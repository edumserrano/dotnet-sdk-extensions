using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public static partial class RunUntilExtensions
    {
        /// <summary>
        /// Terminates the host after the specified timeout.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
        /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> that creates the Host to terminate after the timeout.</param>
        /// <param name="timeout">Timeout value.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilTimeoutAsync<T>(this WebApplicationFactory<T> webApplicationFactory, TimeSpan timeout) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));

            RunUntilPredicateAsync noOpPredicateAsync = () => Task.FromResult(false);
            var options = new RunUntilOptions { Timeout = timeout };
            var hostRunner = new WebApplicationFactoryHostRunner<T>(webApplicationFactory);
            return hostRunner.RunUntilAsync(noOpPredicateAsync, options, throwExceptionIfTimeout: false);
        }

        /// <summary>
        /// Terminates the host after the specified timeout.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to terminate after the timeout.</param>
        /// <param name="timeout">Timeout value.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilTimeoutAsync(this IHost host, TimeSpan timeout)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));

            RunUntilPredicateAsync noOpPredicateAsync = () => Task.FromResult(false);
            var options = new RunUntilOptions { Timeout = timeout };
            var hostRunner = new DefaultHostRunner(host);
            return hostRunner.RunUntilAsync(noOpPredicateAsync, options, throwExceptionIfTimeout: false);
        }
        
        /*
        * When RunUntilAsync method is awaited the webserver will stay running until:
        * - the Host exits (ie: crashes or graceful shutdown)
        * - the predicate returns true and the server is disposed (non graceful shutdown)
        * - the timeout on the RunUntilOptions expires and the the server is disposed (non graceful shutdown)
        */
        internal static async Task RunUntilAsync(
            this HostRunner hostRunner,
            RunUntilPredicateAsync predicateAsync,
            RunUntilOptions options,
            bool throwExceptionIfTimeout)
        {
            if (hostRunner == null) throw new ArgumentNullException(nameof(hostRunner));
            if (options == null) throw new ArgumentNullException(nameof(options));

            await hostRunner.StartAsync(); 
            var hostRunController = new HostRunController(options);
            var runUntilResult = await hostRunController.RunUntil(predicateAsync);
            await hostRunner.StopAsync();
            hostRunner.Dispose(); 
            if (runUntilResult == RunUntilResult.TimedOut && throwExceptionIfTimeout)
            {
                throw new RunUntilException($"{nameof(RunUntilExtensions)}.{nameof(RunUntilAsync)} timed out after {options.Timeout}. This usually means the test server was shutdown before the {nameof(RunUntilExtensions)}.{nameof(RunUntilAsync)} predicate returned true. If you want to run the server for a period of time consider using {nameof(RunUntilExtensions)}.{nameof(RunUntilTimeoutAsync)} instead");
            }
        }
    }
}