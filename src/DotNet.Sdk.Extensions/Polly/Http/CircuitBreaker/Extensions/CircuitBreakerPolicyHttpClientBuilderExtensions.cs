using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions
{
    /// <summary>
    /// Provides methods to add a circuit breaker policy to an <see cref="HttpClient"/> via the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class CircuitBreakerPolicyHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker policy to.</param>
        /// <param name="optionsName">The name of the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker policy.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

            static ICircuitBreakerPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultCircuitBreakerPolicyEventHandler();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: EventHandlerFactory);
        }

        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker policy to.</param>
        /// <param name="configureOptions">An action to define the the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker policy.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

            static ICircuitBreakerPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultCircuitBreakerPolicyEventHandler();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: EventHandlerFactory);
        }

        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <typeparam name="TPolicyEventHandler">The type that will handle circuit breaker events.</typeparam>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker policy to.</param>
        /// <param name="optionsName">The name of the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker policy.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyEventHandler
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            static ICircuitBreakerPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: EventHandlerFactory);
        }

        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <typeparam name="TPolicyEventHandler">The type that will handle circuit breaker  events.</typeparam>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker  policy to.</param>
        /// <param name="configureOptions">An action to define the the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker  policy.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyEventHandler
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            static ICircuitBreakerPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: EventHandlerFactory);
        }

        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker policy to.</param>
        /// <param name="optionsName">The name of the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker policy.</param>
        /// <param name="eventHandlerFactory">Delegate to create an instance that will handle circuit breaker events.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory)
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

            return httpClientBuilder.AddCircuitBreakerPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        /// <summary>
        /// Adds a circuit breaker policy to the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the circuit breaker policy to.</param>
        /// <param name="configureOptions">An action to define the the <see cref="CircuitBreakerOptions"/> options to use to configure the circuit breaker policy.</param>
        /// <param name="eventHandlerFactory">Delegate to create an instance that will handle circuit breaker events.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<CircuitBreakerOptions> configureOptions,
            Func<IServiceProvider, ICircuitBreakerPolicyEventHandler> eventHandlerFactory)
        {
            if (httpClientBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpClientBuilder));
            }

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
                var clientCircuitBreakerOptions = provider.GetHttpClientCircuitBreakerOptions(optionsName);
                var circuitBreakerPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(httpClientName, clientCircuitBreakerOptions, policyEventHandler);
                return new PolicyHttpMessageHandler(circuitBreakerPolicy);
            });
        }
    }
}
