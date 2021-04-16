using System;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Retry
{
    internal static class RetryPolicyFactory
    {
        public static IsPolicy CreateRetryPolicy(
            RetryOptions options,
            IRetryPolicyConfiguration policyConfiguration)
        {
            var medianFirstRetryDelay = TimeSpan.FromSeconds(options.MedianFirstRetryDelayInSecs);
            var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, options.RetryCount);
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    sleepDurations: retryDelays,
                    onRetryAsync: (outcome, retryDelay, retryNumber, pollyContext) =>
                    {
                        return policyConfiguration.OnRetryAsync(options, outcome, retryDelay, retryNumber, pollyContext);
                    });
            return policy;
        }
    }
}
