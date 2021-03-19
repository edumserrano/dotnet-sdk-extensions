using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public interface IRetryPolicyConfiguration
    {
        Task OnRetry(
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan retryDelay,
            int retryNumber,
            Context pollyContext);
    }
}
