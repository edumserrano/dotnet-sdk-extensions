using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Polly;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly
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

        public static TimeoutPolicyConfiguration GetPolicyConfiguration<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return new TimeoutPolicyConfiguration(policy);
        }

        public static TestTimeoutPolicyConfiguration GetPolicyConfiguration2<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return policy
                .GetInstanceField("_onTimeoutAsync")
                .GetInstanceProperty("Target")
                .GetInstanceField<TestTimeoutPolicyConfiguration>("policyConfiguration");
        }

        public static string GetPolicyConfigurationHttpClientName<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return policy
                .GetInstanceField("_onTimeoutAsync")
                .GetInstanceProperty("Target")
                .GetInstanceField<string>("httpClientName");
        }

        public static TimeoutOptions GetPolicyConfigurationTimeoutOptions<T>(this AsyncTimeoutPolicy<T> policy)
        {
            return policy
                .GetInstanceField("_onTimeoutAsync")
                .GetInstanceProperty("Target")
                .GetInstanceField<TimeoutOptions>("options");
        }

        public class TimeoutPolicyConfiguration
        {
            public TimeoutPolicyConfiguration(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onTimeoutAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                TimeoutOptions = onTimeoutAsync.GetInstanceField<TimeoutOptions>("options");
                PolicyConfiguration = onTimeoutAsync.GetInstanceField<ITimeoutPolicyConfiguration>("policyConfiguration");
            }

            public string HttpClientName { get; }

            public TimeoutOptions TimeoutOptions { get; }

            public ITimeoutPolicyConfiguration PolicyConfiguration { get; }
        }
    }
}