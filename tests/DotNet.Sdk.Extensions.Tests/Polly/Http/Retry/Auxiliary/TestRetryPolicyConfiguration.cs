using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using Polly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class TestRetryPolicyConfiguration : IRetryPolicyConfiguration
    {
        public Task OnRetryAsync(
            RetryOptions retryOptions, 
            DelegateResult<HttpResponseMessage> outcome, 
            TimeSpan retryDelay, 
            int retryNumber,
            Context pollyContext)
        {
            return Task.CompletedTask;
        }
    }
}
