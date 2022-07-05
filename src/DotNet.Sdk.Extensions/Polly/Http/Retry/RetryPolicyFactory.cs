using System.Net;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry;

internal static class RetryPolicyFactory
{
    public static AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(
        string httpClientName,
        RetryOptions options,
        IRetryPolicyEventHandler policyEventHandler)
    {
        var medianFirstRetryDelay = TimeSpan.FromSeconds(options.MedianFirstRetryDelayInSecs);
        var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, options.RetryCount);
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>() // returned by the timeout policy when timeout occurs
            .Or<TaskCanceledException>()
            .OrResult(response =>
            {
                // handle transient http status codes except if it's a CircuitBrokenHttpResponseMessage
                // if the circuit is open no point in retrying
                if (response is CircuitBrokenHttpResponseMessage)
                {
                    return false;
                }

                // transient http status codes: 5xx or 408
                return response.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;
            })
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
