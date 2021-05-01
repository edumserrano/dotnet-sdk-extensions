using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
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

        public static OnTimeoutTarget GetOnTimeoutTarget<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return new OnTimeoutTarget(policy);
        }
        
        public class OnTimeoutTarget
        {
            public OnTimeoutTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onTimeoutAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                TimeoutOptions = onTimeoutAsync.GetInstanceField<TimeoutOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<ITimeoutPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public TimeoutOptions TimeoutOptions { get; }

            public ITimeoutPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}