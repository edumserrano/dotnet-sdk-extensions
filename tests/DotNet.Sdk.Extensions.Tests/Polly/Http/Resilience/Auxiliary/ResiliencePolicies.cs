using System.Collections.Generic;
using System.Net.Http;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary
{
    internal class ResiliencePolicies
    {
        public ResiliencePolicies(List<PolicyHttpMessageHandler> policyHttpMessageHandlers)
        {
            FallbackPolicy = policyHttpMessageHandlers[0].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>();
            RetryPolicy = policyHttpMessageHandlers[1].GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>();
            CircuitBreakerPolicy = policyHttpMessageHandlers[2].GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>();
            TimeoutPolicy = policyHttpMessageHandlers[3].GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>();
        }

        public AsyncPolicyWrap<HttpResponseMessage> CircuitBreakerPolicy { get; set; }

        public AsyncPolicyWrap<HttpResponseMessage> FallbackPolicy { get; }

        public AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; }

        public AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy { get; }
    }
}