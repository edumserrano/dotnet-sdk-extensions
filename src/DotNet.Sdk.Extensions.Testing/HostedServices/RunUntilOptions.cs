using System;
using System.Diagnostics;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public class RunUntilOptions
    {
        /// <summary>
        /// Period after which the host executing the hosted service will be terminated. Defaults to 5 seconds.
        /// </summary>
        /// <remarks>
        /// When the debbuger is attached (eg: debugging tests) the default timeout is set to be very large
        /// (1 day) so that it doesn't affect the debugging experience: 
        /// eg: tests failing because the host was stopped due to timeout being reached whilst debugging.
        ///
        /// You can always set this timeout explicitly and override the default even when debugging.
        /// </remarks>
        public TimeSpan Timeout { get; set; } = Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(5);

        /// <summary>
        /// Interval of time to check the predicate for the to determine if the host running the hosted service
        /// should be terminated.
        /// </summary>
        public TimeSpan PredicateCheckInterval { get; set; } = TimeSpan.FromMilliseconds(5);
    }
}