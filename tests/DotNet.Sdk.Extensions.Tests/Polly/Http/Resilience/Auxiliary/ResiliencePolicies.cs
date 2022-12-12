namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;

internal sealed class ResiliencePolicies
{
    public ResiliencePolicies(List<PolicyHttpMessageHandler> policyHttpMessageHandlers)
    {
        FallbackPolicy = policyHttpMessageHandlers[0].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>() ?? throw new ArgumentException("Missing fallback policy", nameof(policyHttpMessageHandlers));
        RetryPolicy = policyHttpMessageHandlers[1].GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>() ?? throw new ArgumentException("Missing retry policy", nameof(policyHttpMessageHandlers));
        CircuitBreakerPolicy = policyHttpMessageHandlers[2].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>() ?? throw new ArgumentException("Missing circuit breaker policy", nameof(policyHttpMessageHandlers));
        TimeoutPolicy = policyHttpMessageHandlers[3].GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>() ?? throw new ArgumentException("Missing timeout policy", nameof(policyHttpMessageHandlers));
    }

    public AsyncPolicyWrap<HttpResponseMessage> CircuitBreakerPolicy { get; set; }

    public AsyncPolicyWrap<HttpResponseMessage> FallbackPolicy { get; }

    public AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; }

    public AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy { get; }
}
