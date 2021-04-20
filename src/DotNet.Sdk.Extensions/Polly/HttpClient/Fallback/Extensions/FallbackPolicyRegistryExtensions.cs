using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.Extensions
{
    public static class FallbackPolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientFallbackPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientFallbackPolicy<DefaultFallbackPolicyConfiguration>(policyKey, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientFallbackPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, IFallbackPolicyConfiguration
        {
            // by design TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            return registry.AddHttpClientFallbackPolicy(policyKey,policyConfiguration);
        }

        public static IPolicyRegistry<string> AddHttpClientFallbackPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            IFallbackPolicyConfiguration policyConfiguration)
        {
            var policy = FallbackPolicyFactory.CreateFallbackPolicy(policyConfiguration);
            registry.Add(policyKey, policy);
            return registry;
        }
    }
}

