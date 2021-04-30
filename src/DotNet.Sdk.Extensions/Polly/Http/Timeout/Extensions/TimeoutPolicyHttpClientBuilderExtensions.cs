using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions
{
    public static class TimeoutPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            return httpClientBuilder.AddTimeoutPolicyCore<DefaultTimeoutPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
        {
            return httpClientBuilder.AddTimeoutPolicyCore<DefaultTimeoutPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            return httpClientBuilder.AddTimeoutPolicyCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            return httpClientBuilder.AddTimeoutPolicyCore<TPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        private static IHttpClientBuilder AddTimeoutPolicyCore<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<TimeoutOptions>? configureOptions)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_timeout_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyEventHandler>()
                .AddHttpClientTimeoutOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var configuration = provider.GetRequiredService<TPolicyEventHandler>();
                var timeoutOptions = provider.GetHttpClientTimeoutOptions(optionsName);
                var timeoutPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(httpClientName, timeoutOptions, configuration);
                return new PolicyHttpMessageHandler(timeoutPolicy);
            });
        }
    }
}
