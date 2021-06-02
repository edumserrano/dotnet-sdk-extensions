using System;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions
{
    public static class RetryPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory = _ => new DefaultRetryPolicyEventHandler();
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<RetryOptions> configureOptions)
        {
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory = _ => new DefaultRetryPolicyEventHandler();
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<RetryOptions> configureOptions)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<RetryOptions> configureOptions,
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddRetryPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        private static IHttpClientBuilder AddRetryPolicyCore(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<RetryOptions>? configureOptions,
            Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_retry_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services              
                .AddHttpClientRetryOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyEventHandler = eventHandlerFactory(provider);
                var retryOptions = provider.GetHttpClientRetryOptions(optionsName);
                var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(httpClientName, retryOptions, policyEventHandler);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }
    }
}
