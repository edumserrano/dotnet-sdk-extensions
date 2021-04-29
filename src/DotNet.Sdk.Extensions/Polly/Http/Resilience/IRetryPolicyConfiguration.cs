using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience
{
    public interface IResiliencePolicyConfiguration : 
        ITimeoutPolicyConfiguration, 
        IRetryPolicyConfiguration, 
        ICircuitBreakerPolicyConfiguration, 
        IFallbackPolicyConfiguration
    {

    }
}
