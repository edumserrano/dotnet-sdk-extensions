namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;

/// <summary>
/// Defines the events produced by the HttpClient's resilience policies.
/// </summary>
public interface IResiliencePoliciesEventHandler :
    ITimeoutPolicyEventHandler,
    IRetryPolicyEventHandler,
    ICircuitBreakerPolicyEventHandler,
    IFallbackPolicyEventHandler;
