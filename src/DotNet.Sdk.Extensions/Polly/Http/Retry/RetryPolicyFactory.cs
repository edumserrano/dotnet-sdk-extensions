using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
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
            IRetryPolicyEventHandler policyEventHandler)
        {
            var medianFirstRetryDelay = TimeSpan.FromSeconds(options.MedianFirstRetryDelayInSecs);
            var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, options.RetryCount);
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response is CircuitBrokenHttpResponseMessage) // returned by the CircuitBreakerCheckerAsyncPolicy when circuit is not open/isolated
                .Or<TimeoutRejectedException>() // returned by the timeout policy when timeout occurs
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
                        return policyEventHandler.OnRetryAsync(retryEvent);
                    });
        }
    }
}
