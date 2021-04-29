using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public static class HttpTimeoutPolicyExtensions
    {
        // Forces AsyncTimeoutPolicy to timeout when executing
        public static async Task<PolicyResult<HttpResponseMessage>> ExecuteAndCaptureWithForcedTimeout(
            this AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy,
            int timeoutInSecs)
        {
            var cts = new CancellationTokenSource();
            var policyResult = await timeoutPolicy.ExecuteAndCaptureAsync(
                action: async (context, cancellationToken) =>
                {
                    var timeoutSpan = TimeSpan.FromSeconds(timeoutInSecs);
                    cts.CancelAfter(timeoutSpan);
                    await Task.Delay(timeoutSpan, cancellationToken);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                },
                context: new Context(),
                cancellationToken: cts.Token);
            return policyResult;
        }
    }
}