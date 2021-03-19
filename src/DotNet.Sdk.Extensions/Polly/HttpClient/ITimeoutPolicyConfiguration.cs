using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public interface ITimeoutPolicyConfiguration
    {
        Task OnTimeout(
            TimeoutOptions timeoutOptions,
            Context context, 
            TimeSpan requestTimeout, 
            Task timedOutTask, 
            Exception exception);
    }
}