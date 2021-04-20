using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions
{
    public static class RetryPolicyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientRetryPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientRetryPolicy<DefaultRetryPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientRetryPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, IRetryPolicyConfiguration
        {
            // by design TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            return registry.AddHttpClientRetryPolicy(policyKey, optionsName, policyConfiguration, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientRetryPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IRetryPolicyConfiguration policyConfiguration,
            IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetHttpClientRetryOptions(optionsName);
            return registry.AddHttpClientRetryPolicy(policyKey, options, policyConfiguration);
        }

        public static IPolicyRegistry<string> AddHttpClientRetryPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            RetryOptions options,
            IRetryPolicyConfiguration policyConfiguration)
        {
            var policy = RetryPolicyFactory.CreateRetryPolicy(options, policyConfiguration);
            registry.Add(policyKey, policy);
            return registry;
        }
    }
}

