using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
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

        public static IPolicyRegistry<string> AddHttpClientTimeoutPolicy<TPolicyConfiguration>(
            this IPolicyRegistry<string> registry,
            string policyKey,
            string optionsName,
            IServiceProvider serviceProvider) where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
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
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CircuitBreakerOptions>>();
            var options = optionsMonitor.Get(optionsName);
            var circuitBreakerPolicy = HttpPolicyExtensions
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
                        await policyConfiguration.OnBreakAsync(options, lastOutcome, previousState, breakDuration, context);
                    },
                    onReset: async context =>
                    {
                        await policyConfiguration.OnResetAsync(options, context);
                    },
                    onHalfOpen: async () =>
                    {
                        await policyConfiguration.OnHalfOpenAsync(options);
                    });

            var circuitBreakerCheckerPolicy = new CircuitBreakerCheckerAsyncPolicy(circuitBreakerPolicy);
            var finalPolicy = Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
            registry.Add(key: policyKey, finalPolicy);
            return registry;
        }
        
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
            // by choice, TPolicyConfiguration is not added to the IServiceCollection so use ActivatorUtilities  instead of IServiceProvider.GetRequiredService<T>
            var policyConfiguration = ActivatorUtilities.CreateInstance<TPolicyConfiguration>(serviceProvider);
            var timeoutFallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new TimeoutHttpResponseMessage(),
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnTimeoutFallbackAsync(outcome, context);
                    });
            var brokenCircuitFallback = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackValue: new CircuitBrokenHttpResponseMessage(),
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnBrokenCircuitFallbackAsync(outcome, context);
                    });
            var abortedFallback = Policy<HttpResponseMessage>
                .Handle<TaskCanceledException>()
                .FallbackAsync(
                    fallbackValue: new AbortedHttpResponseMessage(),
                    onFallbackAsync: (outcome, context) =>
                    {
                        return policyConfiguration.OnTaskCancelledFallbackAsync(outcome, context);
                    });
            var policy = Policy.WrapAsync(timeoutFallback, brokenCircuitFallback, abortedFallback);
            registry.Add(key: policyKey, policy);
            return registry;
        }
    }
}

