using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /// <summary>
    /// Provides extension methods for the RunUntil method on IHost where the predicate is an async function.
    /// </summary>
    public static partial class RunUntilExtensions
    {
        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to execute.</param>
        /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync(
            this IHost host,
            RunUntilPredicateAsync predicateAsync)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
            return host.RunUntilAsync(predicateAsync, configureOptionsAction);

            static void DefaultConfigureOptionsDelegate(RunUntilOptions _)
            {
                // default configure options delegate == do nothing
                // use default values of the RunUntilOptions
            }
        }

        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to execute.</param>
        /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
        /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync(
            this IHost host,
            RunUntilPredicateAsync predicateAsync,
            Action<RunUntilOptions> configureOptions)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var defaultOptions = new RunUntilOptions();
            configureOptions(defaultOptions);
            var hostRunner = new DefaultHostRunner(host);
            return hostRunner.RunUntilAsync(predicateAsync, defaultOptions);
        }
    }
}
