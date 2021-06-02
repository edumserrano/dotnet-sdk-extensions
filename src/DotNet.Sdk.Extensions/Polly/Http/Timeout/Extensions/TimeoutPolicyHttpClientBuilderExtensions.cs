using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions
{
    public static class TimeoutPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory = _ => new DefaultTimeoutPolicyEventHandler();
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
        {
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory = _ => new DefaultTimeoutPolicyEventHandler();
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: optionsName,
                configureOptions: null,
                eventHandlerFactory: eventHandlerFactory);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions,
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
        {
            return httpClientBuilder.AddTimeoutPolicyCore(
                optionsName: null,
                configureOptions: configureOptions,
                eventHandlerFactory: eventHandlerFactory);
        }
        
        private static IHttpClientBuilder AddTimeoutPolicyCore(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<TimeoutOptions>? configureOptions,
            Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_timeout_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddHttpClientTimeoutOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var configuration = eventHandlerFactory(provider);
                var timeoutOptions = provider.GetHttpClientTimeoutOptions(optionsName);
                var timeoutPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(httpClientName, timeoutOptions, configuration);
                return new PolicyHttpMessageHandler(timeoutPolicy);
            });
        }
    }
}
