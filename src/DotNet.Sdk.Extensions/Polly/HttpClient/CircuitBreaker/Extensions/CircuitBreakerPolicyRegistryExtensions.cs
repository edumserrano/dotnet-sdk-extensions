using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

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
    }
}

