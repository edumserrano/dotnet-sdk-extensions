using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration
{
    public interface IResiliencePolicyConfiguration : 
        ITimeoutPolicyConfiguration, 
        IRetryPolicyConfiguration, 
        ICircuitBreakerPolicyConfiguration, 
        IFallbackPolicyConfiguration
    {

    }
}
