using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

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
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RetryOptions>>();
            var options = optionsMonitor.Get(optionsName);
            var medianFirstRetryDelay = TimeSpan.FromSeconds(options.MedianFirstRetryDelayInSecs);
            var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, options.RetryCount);
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    sleepDurations: retryDelays,
                    onRetryAsync: (outcome, retryDelay, retryNumber, pollyContext) =>
                     {
                         return policyConfiguration.OnRetryAsync(options, outcome, retryDelay, retryNumber, pollyContext);
                     });
            registry.Add(key: policyKey, policy);
            return registry;
        }
    }
}

