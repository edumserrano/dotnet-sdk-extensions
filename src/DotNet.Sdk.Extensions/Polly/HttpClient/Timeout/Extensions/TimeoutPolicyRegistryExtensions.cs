using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
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
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TimeoutOptions>>();
            var options = optionsMonitor.Get(optionsName);
            var policy = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(options.TimeoutInSecs),
                onTimeoutAsync: (context, requestTimeout, timedOutTask, exception) =>
                 {
                     return policyConfiguration.OnTimeoutASync(options, context, requestTimeout, timedOutTask, exception);
                 });
            registry.Add(key: policyKey, policy);
            return registry;
        }
    }
}

