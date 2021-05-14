using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    /// <summary>
    /// This class uses composition with the other asserter classes to avoid repeating the
    /// assertion logic that makes sure a policy works as expected.
    ///
    /// This is done in this way because the Resilience Policies is a wrapped policy for several policies
    /// that have their own tests so we can re-use the assertion logic from those tests.
    /// </summary>
    internal class ResiliencePoliciesAsserter
    {
        private readonly RetryPolicyAsserter _retryPolicyAsserter;
        private readonly TimeoutPolicyAsserter _timeoutPolicyAsserter;

        public ResiliencePoliciesAsserter(
            HttpClient httpClient,
            ResilienceOptions resilienceOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _retryPolicyAsserter = new RetryPolicyAsserter(
                httpClient,
                resilienceOptions.Retry,
                testHttpMessageHandler);
            _timeoutPolicyAsserter = new TimeoutPolicyAsserter(
                httpClient,
                resilienceOptions.Timeout,
                testHttpMessageHandler);
        }

        public TimeoutPolicyAsserter Timeout => _timeoutPolicyAsserter;

        public RetryPolicyAsserter Retry => _retryPolicyAsserter;
    }
}
