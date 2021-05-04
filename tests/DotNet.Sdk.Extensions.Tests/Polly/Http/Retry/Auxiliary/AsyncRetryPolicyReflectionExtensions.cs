using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
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
    }
}