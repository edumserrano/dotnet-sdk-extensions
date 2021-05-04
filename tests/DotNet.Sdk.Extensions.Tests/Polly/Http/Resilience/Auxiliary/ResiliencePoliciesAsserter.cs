using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    internal class ResiliencePoliciesAsserter
    {
        private readonly FallbackPolicyAsserter _fallbackPolicyAsserter;
        private readonly RetryPolicyAsserter _retryPolicyAsserter;
        private readonly CircuitBreakerPolicyAsserter _circuitBreakerPolicyAsserter;

        public ResiliencePoliciesAsserter(
            string httpClientName,
            ResilienceOptions resilienceOptions,
            List<PolicyHttpMessageHandler> policyHttpMessageHandlers)
        {
            var resiliencePolicies = new ResiliencePolicies(policyHttpMessageHandlers);
            _fallbackPolicyAsserter = new FallbackPolicyAsserter(httpClientName, resiliencePolicies.FallbackPolicy);
            _retryPolicyAsserter = new RetryPolicyAsserter(httpClientName, resilienceOptions.Retry, resiliencePolicies.RetryPolicy);
            _circuitBreakerPolicyAsserter = new CircuitBreakerPolicyAsserter(httpClientName, resilienceOptions.CircuitBreaker, resiliencePolicies.CircuitBreakerPolicy);
        }
        
        public void PoliciesShouldBeConfiguredAsExpected()
        {
            _fallbackPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            _retryPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            _circuitBreakerPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
        }

        public void PoliciesShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {
            _fallbackPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(policyEventHandler);
            _retryPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(policyEventHandler);
            _circuitBreakerPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(policyEventHandler);
        }
    }
}
