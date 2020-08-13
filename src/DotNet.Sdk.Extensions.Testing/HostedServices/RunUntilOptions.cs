using System;
using System.Diagnostics;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public class RunUntilOptions
    {
        /*
         * When debugging tests the default timeout is set to be very large so that
         * it doesn't affect the debugging experience (i.e.: tests failing cause the server was stopped due to timeout being reached whilst debugging)
         *
         * You can always set this timeout explicitly for each test and override the default even when debugging.
         *
         */
        public TimeSpan Timeout { get; set; } = Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(5);
        
        public TimeSpan PredicateCheckInterval { get; set; } = TimeSpan.FromMilliseconds(5);
    }
}