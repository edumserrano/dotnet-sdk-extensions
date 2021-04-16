using System;
using System.Threading.Tasks;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout
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