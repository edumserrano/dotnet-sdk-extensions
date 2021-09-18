using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
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
    internal class ResiliencePoliciesAsserter
    {
        public ResiliencePoliciesAsserter(
            HttpClient httpClient,
            ResilienceOptions resilienceOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            Retry = new RetryPolicyAsserter(
                httpClient,
                resilienceOptions.Retry,
                testHttpMessageHandler);
            Timeout = new TimeoutPolicyAsserter(
                httpClient,
                resilienceOptions.Timeout,
                testHttpMessageHandler);
            CircuitBreaker = new CircuitBreakerPolicyAsserter(
                httpClient,
                resilienceOptions.CircuitBreaker,
                testHttpMessageHandler);
            Fallback = new FallbackPolicyAsserter(
                httpClient,
                testHttpMessageHandler);
        }

        public FallbackPolicyAsserter Fallback { get; }

        public TimeoutPolicyAsserter Timeout { get; }

        public RetryPolicyAsserter Retry { get; }

        public CircuitBreakerPolicyAsserter CircuitBreaker { get; }
    }
}
