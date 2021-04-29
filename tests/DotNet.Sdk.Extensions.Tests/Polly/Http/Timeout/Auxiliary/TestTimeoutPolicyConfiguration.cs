using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using Polly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TestTimeoutPolicyConfiguration : ITimeoutPolicyConfiguration
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
