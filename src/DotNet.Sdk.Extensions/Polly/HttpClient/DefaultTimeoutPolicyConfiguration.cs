using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    internal class DefaultTimeoutPolicyConfiguration : ITimeoutPolicyConfiguration
    {
        public Task OnTimeoutASync(
            TimeoutOptions timeoutOptions, 
            Context context,
            TimeSpan requestTimeout,
            Task timedOutTask,
            Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}