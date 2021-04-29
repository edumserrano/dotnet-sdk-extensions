using System;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout
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