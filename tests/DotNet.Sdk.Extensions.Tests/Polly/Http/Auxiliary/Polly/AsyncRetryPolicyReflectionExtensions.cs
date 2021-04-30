using System;
using System.Collections.Generic;
using System.Linq;
using Polly.Retry;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly
{
    public static class AsyncRetryPolicyReflectionExtensions
    {
        public static int GetRetryCount<T>(this AsyncRetryPolicy<T> policy)
        {
            var sleepDurationsEnumerable = policy.GetInstanceField<IEnumerable<TimeSpan>>("_sleepDurationsEnumerable"); // need the cast or need to for a  //((IEnumerable<TimeSpan>) b).ToList() to make sure the properties retrieved next are populated as expected
            var _ = sleepDurationsEnumerable.ToList(); // need to force the evaluation of the enumerable or else the maxRetries will be always zero
            var maxRetries = sleepDurationsEnumerable.GetInstanceField<int>("maxRetries");
            return maxRetries;
        }

        public static TimeSpan GetMedianFirstRetryDelay<T>(this AsyncRetryPolicy<T> policy)
        {
            var sleepDurationsEnumerable = policy.GetInstanceField<IEnumerable<TimeSpan>>("_sleepDurationsEnumerable"); // need the cast or need to for a  //((IEnumerable<TimeSpan>) b).ToList() to make sure the properties retrieved next are populated as expected
            var _ = sleepDurationsEnumerable.ToList(); // need to force the evaluation of the enumerable or else the scaleFirstTry will be always zero
            var scaleFirstTry = sleepDurationsEnumerable.GetInstanceField<TimeSpan>("scaleFirstTry");
            return scaleFirstTry;
        }
    }
}
