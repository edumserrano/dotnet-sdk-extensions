using System;
using System.Diagnostics;

namespace AspNetCore.Extensions.Testing.HostedServices
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
        
        // Not sure what's a good default for this. It seems that very low values might not be indicated
        // I couldn't reproduce consistently but in some tests I was doing they seemed to fail because of this
        // After I've added tests for this I should be able to understand better and delete this comment
        public TimeSpan PredicateCheckInterval { get; set; } = TimeSpan.FromMilliseconds(10);
    }
}