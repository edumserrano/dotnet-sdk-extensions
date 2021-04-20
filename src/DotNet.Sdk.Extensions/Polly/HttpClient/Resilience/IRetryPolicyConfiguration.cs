using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Resilience
{
    public interface IResiliencePolicyConfiguration : 
        ITimeoutPolicyConfiguration, 
        IRetryPolicyConfiguration, 
        ICircuitBreakerPolicyConfiguration, 
        IFallbackPolicyConfiguration
    {

    }
}
