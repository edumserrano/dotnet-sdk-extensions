using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions
{
    public static class CircuitBreakerPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory = _ => new DefaultCircuitBreakerPolicyEventHandler();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
        {
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory = _ => new DefaultCircuitBreakerPolicyEventHandler();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions,
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        private static IHttpClientBuilder AddCircuitBreakerPolicyCore(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<CircuitBreakerOptions>? configureOptions,
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory)
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_circuit_breaker_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyEventHandler = eventHandlerFactory(provider);
                var retryOptions = provider.GetHttpClientCircuitBreakerOptions(optionsName);
                var circuitBreakerPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(httpClientName, retryOptions, policyEventHandler);
                return new PolicyHttpMessageHandler(circuitBreakerPolicy);
            });
        }
    }
}
