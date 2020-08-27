using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /*
     * RunUntil WebApplicationFactory extension methods overloads
     * where the predicate is an async function.
     */
    public static partial class RunUntilWebApplicationFactoryExtensions
    {
        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
        /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
        /// <param name="predicateAsyncAsync">The async predicate to determine when the host should be terminated.</param>
        /// <param name="runUntilCancellationToken">The <see cref="CancellationToken"/> to abort the predicate check if required.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            RunUntilPredicateAsync predicateAsyncAsync,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));

            var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
            return webApplicationFactory.RunUntilAsync(predicateAsyncAsync, configureOptionsAction, runUntilCancellationToken);

            void DefaultConfigureOptionsDelegate(RunUntilOptions obj)
            {
                // default configure options delegate == do nothing
                // use default values of the RunUntilOptions
            }
        }

        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
        /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
        /// <param name="predicateAsyncAsync">The async predicate to determine when the host should be terminated.</param>
        /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
        /// <param name="runUntilCancellationToken">The <see cref="CancellationToken"/> to abort the predicate check if required.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync<T>(
            this WebApplicationFactory<T> webApplicationFactory,
            RunUntilPredicateAsync predicateAsyncAsync,
            Action<RunUntilOptions> configureOptions,
            CancellationToken runUntilCancellationToken = default) where T : class
        {
            if (webApplicationFactory == null) throw new ArgumentNullException(nameof(webApplicationFactory));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var defaultOptions = new RunUntilOptions();
            configureOptions(defaultOptions);
            return webApplicationFactory.RunUntilAsync(predicateAsyncAsync, defaultOptions, throwExceptionIfTimeout: true, runUntilCancellationToken);
        }
    }
}