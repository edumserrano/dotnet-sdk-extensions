namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;

internal static class ResiliencePoliciesAsserterExtensions
{
    public static ResiliencePoliciesAsserter ResiliencePoliciesAsserter(
        this HttpClient httpClient,
        ResilienceOptions options,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new ResiliencePoliciesAsserter(httpClient, options, testHttpMessageHandler);
    }
}

/// <summary>
/// This class uses composition with the other asserter classes to avoid repeating the
/// assertion logic that makes sure a policy works as expected.
///
/// This is done in this way because the Resilience Policies is a wrapped policy for several policies
/// that have their own tests so we can re-use the assertion logic from those tests.
/// </summary>
internal sealed class ResiliencePoliciesAsserter(
    HttpClient httpClient,
    ResilienceOptions resilienceOptions,
    TestHttpMessageHandler testHttpMessageHandler)
{
    public FallbackPolicyAsserter Fallback { get; } = new FallbackPolicyAsserter(
            httpClient,
            testHttpMessageHandler);

    public TimeoutPolicyAsserter Timeout { get; } = new TimeoutPolicyAsserter(
            httpClient,
            resilienceOptions.Timeout,
            testHttpMessageHandler);

    public RetryPolicyAsserter Retry { get; } = new RetryPolicyAsserter(
            httpClient,
            resilienceOptions.Retry,
            testHttpMessageHandler);

    public CircuitBreakerPolicyAsserter CircuitBreaker { get; } = new CircuitBreakerPolicyAsserter(
            httpClient,
            resilienceOptions.CircuitBreaker,
            testHttpMessageHandler);
}
