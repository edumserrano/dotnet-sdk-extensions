using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions
{
    public static class CircuitBreakerHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<DefaultCircuitBreakerPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<DefaultCircuitBreakerPolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<CircuitBreakerOptions> configureOptions)
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<DefaultCircuitBreakerPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        private static IHttpClientBuilder AddCircuitBreakerPolicyCore<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<CircuitBreakerOptions>? configureOptions )
            where TPolicyEventHandler : class, ICircuitBreakerPolicyConfiguration
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_circuit_breaker_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyEventHandler>()
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var retryOptions = provider.GetHttpClientCircuitBreakerOptions(optionsName);
                var circuitBreakerPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(httpClientName, retryOptions, policyConfiguration);
                return new PolicyHttpMessageHandler(circuitBreakerPolicy);
            });
        }
    }
}
