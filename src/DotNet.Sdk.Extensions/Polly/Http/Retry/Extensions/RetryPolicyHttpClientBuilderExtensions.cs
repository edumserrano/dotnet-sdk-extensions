using System;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions
{
    public static class RetryPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            return httpClientBuilder.AddRetryPolicyCore<DefaultRetryPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Action<RetryOptions> configureOptions)
        {
            return httpClientBuilder.AddRetryPolicyCore<DefaultRetryPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            return httpClientBuilder.AddRetryPolicyCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<RetryOptions> configureOptions)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            return httpClientBuilder.AddRetryPolicyCore<TPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        private static IHttpClientBuilder AddRetryPolicyCore<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName,
            Action<RetryOptions>? configureOptions)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_retry_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyEventHandler>()
                .AddHttpClientRetryOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var retryOptions = provider.GetHttpClientRetryOptions(optionsName);
                var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(httpClientName, retryOptions, policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }
    }
}
