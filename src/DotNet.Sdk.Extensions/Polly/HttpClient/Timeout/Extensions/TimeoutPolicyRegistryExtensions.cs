using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions
{
    public static class TimeoutPolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientTimeoutPolicy<DefaultTimeoutPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            return registry.AddHttpClientTimeoutPolicy(policyKey, optionsName, policyConfiguration, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            ITimeoutPolicyConfiguration policyConfiguration,
            IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetHttpClientTimeoutOptions(optionsName);
            return registry.AddHttpClientTimeoutPolicy(policyKey, options, policyConfiguration);
        }

        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            TimeoutOptions options,
            ITimeoutPolicyConfiguration policyConfiguration)
        {
            var policy = TimeoutPolicyFactory.CreateTimeoutPolicy(options, policyConfiguration);
            registry.Add(policyKey, policy);
            return registry;
        }
    }
}

