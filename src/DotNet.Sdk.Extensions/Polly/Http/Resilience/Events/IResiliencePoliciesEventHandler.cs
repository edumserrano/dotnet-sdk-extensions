using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Events
{
    /// <summary>
    /// Defines the events produced by the HttpClient's resilience policies.
    /// </summary>
    public interface IResiliencePoliciesEventHandler :
        ITimeoutPolicyEventHandler,
        IRetryPolicyEventHandler,
        ICircuitBreakerPolicyEventHandler,
        IFallbackPolicyEventHandler
    {
    }
}
