using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
using Polly.Retry;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public static class AsyncRetryPolicyReflectionExtensions
    {
        public static int GetRetryCount<T>(this AsyncRetryPolicy<T> policy)
        {
            return policy
                .GetSleepDurationsEnumerable()
                .GetInstanceField<int>("maxRetries");
        }

        public static TimeSpan GetMedianFirstRetryDelay<T>(this AsyncRetryPolicy<T> policy)
        {
            return policy
                .GetSleepDurationsEnumerable()
                .GetInstanceField<TimeSpan>("scaleFirstTry");
        }

        private static IEnumerable<TimeSpan> GetSleepDurationsEnumerable<T>(this AsyncRetryPolicy<T> policy)
        {
            var sleepDurationsEnumerable = policy.GetInstanceField<IEnumerable<TimeSpan>>("_sleepDurationsEnumerable");
            var _ = sleepDurationsEnumerable.ToList(); // need to force the evaluation of the enumerable or else the instance fields that I want to retrieve from it will be always zero
            return sleepDurationsEnumerable;
        }
        
        public static OnRetryTarget GetOnRetryTarget<T>(this AsyncRetryPolicy<T> policy)
        {
            return new OnRetryTarget(policy);
        }
        
        public class OnRetryTarget
        {
            public OnRetryTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onRetryAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                RetryOptions = onTimeoutAsync.GetInstanceField<RetryOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<IRetryPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public RetryOptions RetryOptions { get; }

            public IRetryPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}