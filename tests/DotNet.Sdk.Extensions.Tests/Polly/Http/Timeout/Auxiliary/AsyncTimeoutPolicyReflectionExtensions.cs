using System;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public static class AsyncTimeoutPolicyReflectionExtensions
    {
        public static TimeoutStrategy GetTimeoutStrategy<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return policy.GetInstanceField<TimeoutStrategy>("_timeoutStrategy");
        }

        public static TimeSpan GetTimeout<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return policy
                .GetInstanceField("_timeoutProvider")
                .GetInstanceProperty("Target")
                .GetInstanceField<TimeSpan>("timeout");
        }
    }
}