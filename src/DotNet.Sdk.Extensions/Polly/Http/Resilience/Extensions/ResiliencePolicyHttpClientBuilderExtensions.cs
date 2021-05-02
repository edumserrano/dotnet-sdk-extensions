using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions
{
    public static class ResiliencePolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyEventReceiver>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyEventReceiver>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<ResilienceOptions> configureOptions)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyEventReceiver>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IResiliencePolicyEventReceiver
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
            where TPolicyEventHandler : class, IResiliencePolicyEventReceiver
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<ResilienceOptions> configureOptions)
            where TPolicyEventHandler : class, IResiliencePolicyEventReceiver
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        private static IHttpClientBuilder AddResiliencePoliciesCore<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName = null,
            Action<ResilienceOptions>? configureOptions = null)
            where TPolicyEventHandler : class, IResiliencePolicyEventReceiver
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_resilience_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyEventHandler>()
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(configureOptions);
            
            return httpClientBuilder
                .AddFallbackPolicy<TPolicyEventHandler>()
                .AddResilienceRetryPolicy<TPolicyEventHandler>(optionsName)
                .AddResilienceCircuitBreakerPolicy<TPolicyEventHandler>(optionsName)
                .AddResilienceTimeoutPolicy<TPolicyEventHandler>(optionsName);
        }

        private static IHttpClientBuilder AddResilienceRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.Retry,
                    policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        } 
        
        private static IHttpClientBuilder AddResilienceCircuitBreakerPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ICircuitBreakerPolicyEventHandler
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.CircuitBreaker,
                    policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }

        private static IHttpClientBuilder AddResilienceTimeoutPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(
                    httpClientBuilder.Name,
                    resilienceOptions.Timeout,
                    policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }
    }
}
