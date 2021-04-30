using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration
{
    public interface IResiliencePolicyEventReceiver : 
        ITimeoutPolicyEventHandler, 
        IRetryPolicyConfiguration, 
        ICircuitBreakerPolicyConfiguration, 
        IFallbackPolicyConfiguration
    {

    }
}
