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

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        private static IHttpClientBuilder AddCircuitBreakerPolicyCore<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<CircuitBreakerOptions>? configureOptions )
            where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_circuit_breaker_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyConfiguration>()
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyConfiguration>();
                var retryOptions = provider.GetHttpClientCircuitBreakerOptions(optionsName);
                var circuitBreakerPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(httpClientName, retryOptions, policyConfiguration);
                return new PolicyHttpMessageHandler(circuitBreakerPolicy);
            });
        }
    }
}
