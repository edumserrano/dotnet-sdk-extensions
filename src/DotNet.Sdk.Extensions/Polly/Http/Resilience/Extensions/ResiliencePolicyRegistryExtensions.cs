using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions
{
    public static class ResiliencePolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientResiliencePolicies(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientResiliencePolicies<DefaultResiliencePolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientResiliencePolicies<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, IResiliencePolicyConfiguration
        {
            // by design TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            return registry.AddHttpClientResiliencePolicies(policyKey, optionsName, policyConfiguration, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientResiliencePolicies(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IResiliencePolicyConfiguration policyConfiguration,
            IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetHttpClientResilienceOptions(optionsName);
            return registry.AddHttpClientResiliencePolicies(policyKey, options, policyConfiguration);
        }

        public static IPolicyRegistry<string> AddHttpClientResiliencePolicies(
            this IPolicyRegistry<string> registry,
            string policyKey,
            ResilienceOptions options,
            IResiliencePolicyConfiguration policyConfiguration)
        {
            var timeoutPolicyKey = ResiliencePolicyKeys.Timeout(policyKey);
            var retryPolicyKey = ResiliencePolicyKeys.Retry(policyKey);
            var circuitBreakerPolicyKey = ResiliencePolicyKeys.CircuitBreaker(policyKey);
            var fallbackPolicyKey = ResiliencePolicyKeys.Fallback(policyKey);
            registry.AddHttpClientTimeoutPolicy(timeoutPolicyKey, options.Timeout, policyConfiguration);
            registry.AddHttpClientRetryPolicy(retryPolicyKey, options.Retry, policyConfiguration);
            registry.AddHttpClientCircuitBreakerPolicy(circuitBreakerPolicyKey, options.CircuitBreaker, policyConfiguration);
            registry.AddHttpClientFallbackPolicy(fallbackPolicyKey, policyConfiguration);
            return registry;
        }
    }
}

