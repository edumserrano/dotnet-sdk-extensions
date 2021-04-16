using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions
{
    public static class CircuitBreakerPolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientCircuitBreakerPolicy<DefaultCircuitBreakerPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            return registry.AddHttpClientCircuitBreakerPolicy(policyKey, optionsName, policyConfiguration, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            ICircuitBreakerPolicyConfiguration policyConfiguration,
            IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            var policy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(options, policyConfiguration);
            registry.Add(policyKey, policy);
            return registry;
        }

        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            CircuitBreakerOptions options,
            ICircuitBreakerPolicyConfiguration policyConfiguration)
        {
            var policy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(options, policyConfiguration);
            registry.Add(policyKey, policy);
            return registry;
        }
    }
}

