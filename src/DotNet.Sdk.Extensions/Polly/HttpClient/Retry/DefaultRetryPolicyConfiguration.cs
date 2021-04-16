using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Retry
{
    internal class DefaultRetryPolicyConfiguration : IRetryPolicyConfiguration
    {
        public Task OnRetryAsync(
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome, 
            TimeSpan retryDelay, int retryNumber,
            Context pollyContext)
        {
            return Task.CompletedTask;
        }
    }
}