using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /// <summary>
    /// Provides extension methods for the RunUntil method on IHost where the predicate is a sync function.
    /// </summary>
    public static partial class RunUntilExtensions
    {
        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to execute.</param>
        /// <param name="predicate">The predicate to determine when the host should be terminated.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync(
            this IHost host,
            RunUntilPredicate predicate)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Task<bool> PredicateAsync() => Task.FromResult(predicate());
            return host.RunUntilAsync(PredicateAsync);
        }

        /// <summary>
        /// Executes the host until the predicate or a timeout is met.
        /// </summary>
        /// <param name="host">The <see cref="IHost"/> to execute.</param>
        /// <param name="predicate">The predicate to determine when the host should be terminated.</param>
        /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
        /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
        public static Task RunUntilAsync(
            this IHost host,
            RunUntilPredicate predicate,
            Action<RunUntilOptions> configureOptions)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Task<bool> PredicateAsync() => Task.FromResult(predicate());
            return host.RunUntilAsync(PredicateAsync, configureOptions);
        }
    }
}
