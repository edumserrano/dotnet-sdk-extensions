using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration
{
    public interface IResiliencePolicyEventReceiver : 
        ITimeoutPolicyEventHandler, 
        IRetryPolicyEventHandler, 
        ICircuitBreakerPolicyEventHandler, 
        IFallbackPolicyConfiguration
    {

    }
}
