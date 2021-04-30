using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry
{
    internal static class RetryPolicyFactory
    {
        public static AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(
            string httpClientName,
            RetryOptions options,
            IRetryPolicyConfiguration policyConfiguration)
        {
            var medianFirstRetryDelay = TimeSpan.FromSeconds(options.MedianFirstRetryDelayInSecs);
            var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, options.RetryCount);
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    sleepDurations: retryDelays,
                    onRetryAsync: (outcome, retryDelay, retryNumber, pollyContext) =>
                    {
                        var retryEvent = new RetryEvent(
                            httpClientName,
                            options,
                            outcome,
                            retryDelay,
                            retryNumber,
                            pollyContext);
                        return policyConfiguration.OnRetryAsync(retryEvent);
                    });
        }
    }
}
