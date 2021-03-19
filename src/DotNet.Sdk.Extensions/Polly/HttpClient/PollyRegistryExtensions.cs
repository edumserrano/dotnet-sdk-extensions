using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public static class PollyRegistryExtensions
    {
        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientTimeoutPolicy<DefaultTimeoutPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy<T>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where T : class, ITimeoutPolicyConfiguration
        {
            // by choice, T is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<T>(serviceProvider);
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TimeoutOptions>>();
            var options = optionsMonitor.Get(optionsName);
            var policy = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(options.TimeoutInSecs),
                onTimeoutAsync: (context, requestTimeout, timedOutTask, exception) =>
                 {
                     return policyConfiguration.OnTimeout(options, context, requestTimeout, timedOutTask, exception);
                 });
            registry.Add(key: policyKey, policy);
            return registry;
        }

        public static IPolicyRegistry<string> AddHttpClientRetryPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientRetryPolicy<DefaultRetryPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientRetryPolicy<T>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where T : class, IRetryPolicyConfiguration
        {
            // by choice, T is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<T>(serviceProvider);
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
                         return policyConfiguration.OnRetry(options, outcome, retryDelay, retryNumber, pollyContext);
                     });
            registry.Add(key: policyKey, policy);
            return registry;
        }

        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider)
        {
            return registry.AddHttpClientCircuitBreakerPolicy<DefaultCircuitBreakerPolicyConfiguration>(policyKey, optionsName, serviceProvider);
        }

        public static IPolicyRegistry<string> AddHttpClientCircuitBreakerPolicy<T>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where T : class, ICircuitBreakerPolicyConfiguration
        {
            // by choice, T is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<T>(serviceProvider);
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CircuitBreakerOptions>>();
            var options = optionsMonitor.Get(optionsName);
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: options.FailureThreshold,
                    samplingDuration: TimeSpan.FromSeconds(options.SamplingDurationInSecs),
                    minimumThroughput: options.MinimumThroughput,
                    durationOfBreak: TimeSpan.FromSeconds(options.DurationOfBreakInSecs),
                    onBreak: async (lastOutcome, previousState, breakDuration, context) =>
                    {
                        await policyConfiguration.OnBreak(options, lastOutcome, previousState, breakDuration, context);
                    },
                    onReset: async context =>
                    {
                        await policyConfiguration.OnReset(options, context);
                    },
                    onHalfOpen: async () =>
                    {
                        await policyConfiguration.OnHalfOpen(options);
                    });
            registry.Add(key: policyKey, policy);
            return registry;
        }
    }
}

