using System;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public static class PollyHttpClientServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddRetryHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            IConfiguration configuration)
        {
            return httpClientBuilder.AddRetryHandler(policyKey, optionsBuilder =>
            {
                optionsBuilder.Bind(configuration.GetSection("HttpClients:Default:RetryPolicy"));
            });
        }

        public static IHttpClientBuilder AddRetryHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            Action<OptionsBuilder<RetryOptions>> optionsBuilderAction)
        {
            return httpClientBuilder.AddPolicyHandlerFromRegistry(policyKey, optionsBuilderAction);
        }

        public static IHttpClientBuilder AddTimeoutHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            IConfiguration configuration)
        {
            return httpClientBuilder.AddTimeoutHandler(policyKey, optionsBuilder =>
            {
                optionsBuilder.Bind(configuration.GetSection("HttpClients:Default:TimeoutPolicy"));
            });
        }

        public static IHttpClientBuilder AddTimeoutHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            Action<OptionsBuilder<TimeoutOptions>> optionsBuilderAction)
        {
            return httpClientBuilder.AddPolicyHandlerFromRegistry(policyKey, optionsBuilderAction);
        }

        public static IHttpClientBuilder AddCircuitBreakerHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            IConfiguration configuration)
        {
            return httpClientBuilder.AddCircuitBreakerHandler(policyKey, optionsBuilder =>
            {
                optionsBuilder.Bind(configuration.GetSection("HttpClients:Default:CircuitBreakerPolicy"));
            });
        }

        public static IHttpClientBuilder AddCircuitBreakerHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            Action<OptionsBuilder<CircuitBreakerOptions>> optionsBuilderAction)
        {
            // TODO missing circuit breaker check policy
            return httpClientBuilder
                //.AddPolicyHandlerFromRegistry(policyKey, optionsBuilderAction); //policy to check the circuit breaker before executing it
                .AddPolicyHandlerFromRegistry(policyKey, optionsBuilderAction);
        }

        private static IHttpClientBuilder AddPolicyHandlerFromRegistry<T>(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey,
            Action<OptionsBuilder<T>> optionsBuilderAction) where T : class
        {
            var services = httpClientBuilder.Services;
            var optionsBuilder = services.AddOptions<T>(name: policyKey);
            optionsBuilderAction(optionsBuilder);
            return httpClientBuilder.AddPolicyHandlerFromRegistry(policyKey);
        }
    }
}

