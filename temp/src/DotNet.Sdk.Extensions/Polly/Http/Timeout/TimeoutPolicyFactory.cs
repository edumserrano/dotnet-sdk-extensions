using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Polly;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout
{
    internal static class TimeoutPolicyFactory
    {
        public static AsyncTimeoutPolicy<HttpResponseMessage> CreateTimeoutPolicy(
            string httpClientName,
            TimeoutOptions options,
            ITimeoutPolicyEventHandler policyEventHandler)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(options.TimeoutInSecs),
                onTimeoutAsync: (context, requestTimeout, timedOutTask, exception) =>
                {
                    var timeoutEvent = new TimeoutEvent(
                        httpClientName,
                        options,
                        context,
                        requestTimeout,
                        timedOutTask,
                        exception);
                    return policyEventHandler.OnTimeoutAsync(timeoutEvent);
                });
        }
    }
}
