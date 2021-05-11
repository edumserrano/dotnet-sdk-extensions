using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    internal class ResiliencePoliciesAsserter
    {
        public ResiliencePoliciesAsserter(
            string httpClientName,
            ResilienceOptions resilienceOptions,
            List<PolicyHttpMessageHandler> policyHttpMessageHandlers)
        {
            var resiliencePolicies = new ResiliencePolicies(policyHttpMessageHandlers);
        }

        public void PoliciesShouldBeConfiguredAsExpected()
        {

        }

        public void PoliciesShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {

        }
    }
}
