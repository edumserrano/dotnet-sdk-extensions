using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Configuration;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Configuration;
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
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<ResilienceOptions> configureOptions)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, IResiliencePolicyConfiguration
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
            where TPolicyConfiguration : class, IResiliencePolicyConfiguration
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyConfiguration>(
                optionsName: null,
                configureOptions: configureOptions);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName,
            Action<ResilienceOptions> configureOptions)
            where TPolicyConfiguration : class, IResiliencePolicyConfiguration
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyConfiguration>(
                optionsName: optionsName,
                configureOptions: configureOptions);
        }

        private static IHttpClientBuilder AddResiliencePoliciesCore<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName = null,
            Action<ResilienceOptions>? configureOptions = null)
            where TPolicyConfiguration : class, IResiliencePolicyConfiguration
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_resilience_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyConfiguration>()
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(configureOptions);
            
            return httpClientBuilder
                .AddFallbackPolicy<TPolicyConfiguration>()
                .AddResilienceRetryPolicy<TPolicyConfiguration>(optionsName)
                .AddResilienceCircuitBreakerPolicy<TPolicyConfiguration>(optionsName)
                .AddResilienceTimeoutPolicy<TPolicyConfiguration>(optionsName);
        }

        private static IHttpClientBuilder AddResilienceRetryPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, IRetryPolicyConfiguration
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyConfiguration>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.Retry,
                    policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        } 
        
        private static IHttpClientBuilder AddResilienceCircuitBreakerPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, ICircuitBreakerPolicyConfiguration
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyConfiguration>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.CircuitBreaker,
                    policyConfiguration);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }

        private static IHttpClientBuilder AddResilienceTimeoutPolicy<TPolicyConfiguration>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyConfiguration : class, ITimeoutPolicyConfiguration
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyConfiguration>();
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
