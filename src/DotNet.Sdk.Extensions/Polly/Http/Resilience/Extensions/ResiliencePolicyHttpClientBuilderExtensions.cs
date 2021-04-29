using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions
{
    public static class ResiliencePolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddResiliencePoliciesFromRegistry(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey)
        {
            var timeoutPolicyKey = ResiliencePolicyKeys.Timeout(policyKey);
            var retryPolicyKey = ResiliencePolicyKeys.Retry(policyKey);
            var circuitBreakerPolicyKey = ResiliencePolicyKeys.CircuitBreaker(policyKey);
            var fallbackPolicyKey = ResiliencePolicyKeys.Fallback(policyKey);
            return httpClientBuilder
                .AddPolicyHandlerFromRegistry(policyKey: fallbackPolicyKey)
                .AddPolicyHandlerFromRegistry(policyKey: retryPolicyKey)
                .AddPolicyHandlerFromRegistry(policyKey: circuitBreakerPolicyKey)
                .AddPolicyHandlerFromRegistry(policyKey: timeoutPolicyKey);
        }
    }
}
