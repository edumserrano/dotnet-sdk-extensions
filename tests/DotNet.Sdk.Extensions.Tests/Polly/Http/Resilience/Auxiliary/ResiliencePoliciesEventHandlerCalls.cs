namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;

public class ResiliencePoliciesEventHandlerCalls
{
    public ResiliencePoliciesEventHandlerCalls()
    {
        Timeout = new TimeoutPolicyEventHandlerCalls();
        Retry = new RetryPolicyEventHandlerCalls();
        CircuitBreaker = new CircuitBreakerPolicyEventHandlerCalls();
        Fallback = new FallbackPolicyEventHandlerCalls();
    }

    public TimeoutPolicyEventHandlerCalls Timeout { get; }

    public RetryPolicyEventHandlerCalls Retry { get; }

    public CircuitBreakerPolicyEventHandlerCalls CircuitBreaker { get; }

    public FallbackPolicyEventHandlerCalls Fallback { get; }
}
