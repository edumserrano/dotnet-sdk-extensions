using System;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout
{
    public interface ITimeoutPolicyConfiguration
    {
        Task OnTimeoutASync(
            TimeoutOptions timeoutOptions,
            Context context, 
            TimeSpan requestTimeout, 
            Task timedOutTask, 
            Exception exception);
    }
}