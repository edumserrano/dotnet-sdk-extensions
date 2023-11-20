namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;

internal sealed class ResiliencePolicies(List<PolicyHttpMessageHandler> policyHttpMessageHandlers)
{
    public AsyncPolicyWrap<HttpResponseMessage> CircuitBreakerPolicy { get; set; } = policyHttpMessageHandlers[2].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>() ?? throw new ArgumentException("Missing circuit breaker policy", nameof(policyHttpMessageHandlers));

    public AsyncPolicyWrap<HttpResponseMessage> FallbackPolicy { get; } = policyHttpMessageHandlers[0].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>() ?? throw new ArgumentException("Missing fallback policy", nameof(policyHttpMessageHandlers));

    public AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; } = policyHttpMessageHandlers[1].GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>() ?? throw new ArgumentException("Missing retry policy", nameof(policyHttpMessageHandlers));

    public AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy { get; } = policyHttpMessageHandlers[3].GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>() ?? throw new ArgumentException("Missing timeout policy", nameof(policyHttpMessageHandlers));
}
