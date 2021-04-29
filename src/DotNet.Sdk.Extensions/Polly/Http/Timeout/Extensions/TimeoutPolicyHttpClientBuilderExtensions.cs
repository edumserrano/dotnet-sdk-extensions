using System;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;
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
            return httpClientBuilder.AddTimeoutPolicyCore<DefaultTimeoutPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
        {
            return httpClientBuilder.AddTimeoutPolicyCore<DefaultTimeoutPolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<TimeoutOptions> configureOptions)
        {
            return httpClientBuilder.AddTimeoutPolicyCore<DefaultTimeoutPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            return httpClientBuilder.AddTimeoutPolicyCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            Action<TimeoutOptions> configureOptions)
            where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            return httpClientBuilder.AddTimeoutPolicyCore<TPolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddTimeoutPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<TimeoutOptions> configureOptions)
            where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            return httpClientBuilder.AddTimeoutPolicyCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        private static IHttpClientBuilder AddTimeoutPolicyCore<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<TimeoutOptions>? configureOptions)
            where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_timeout_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyConfiguration>()
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var configuration = provider.GetRequiredService<TPolicyConfiguration>();
                var timeoutOptions = provider.GetHttpClientTimeoutOptions(optionsName);
                var timeoutPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(httpClientName, timeoutOptions, configuration);
                return new PolicyHttpMessageHandler(timeoutPolicy);
            });
        }
    }
}
