using System;
using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout
{
    internal static class TimeoutPolicyFactory
    {
        public static IsPolicy CreateTimeoutPolicy(
            TimeoutOptions options,
            ITimeoutPolicyConfiguration policyConfiguration)
        {
            var policy = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(options.TimeoutInSecs),
                onTimeoutAsync: (context, requestTimeout, timedOutTask, exception) =>
                {
                    return policyConfiguration.OnTimeoutASync(options, context, requestTimeout, timedOutTask, exception);
                });
            return policy;
        }
    }
}
