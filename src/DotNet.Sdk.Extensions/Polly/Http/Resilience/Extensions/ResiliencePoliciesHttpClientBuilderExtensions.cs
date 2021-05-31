using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions
{
    public static class ResiliencePoliciesHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePoliciesEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
        {
            return httpClientBuilder.AddResiliencePoliciesCore<DefaultResiliencePoliciesEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IResiliencePoliciesEventHandler
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyEventHandler>(
                optionsName: optionsName,
                configureOptions: null);
        }

        public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            Action<ResilienceOptions> configureOptions)
            where TPolicyEventHandler : class, IResiliencePoliciesEventHandler
        {
            return httpClientBuilder.AddResiliencePoliciesCore<TPolicyEventHandler>(
                optionsName: null,
                configureOptions: configureOptions);
        }
        
        private static IHttpClientBuilder AddResiliencePoliciesCore<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string? optionsName = null,
            Action<ResilienceOptions>? configureOptions = null)
            where TPolicyEventHandler : class, IResiliencePoliciesEventHandler
        {
            var httpClientName = httpClientBuilder.Name;
            optionsName ??= $"{httpClientName}_resilience_{Guid.NewGuid()}";
            configureOptions ??= _ => { };
            httpClientBuilder.Services
                .AddSingleton<TPolicyEventHandler>()
                .AddSingleton<IValidateOptions<ResilienceOptions>, ResilienceOptionsValidation>()
                .AddHttpClientResilienceOptions(optionsName)
                .ValidateDataAnnotations()
                .Configure(configureOptions);

            // here can NOT reuse the other extension methods to add the policies
            // like for instance RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy
            // because those extension methods add the TPolicyEventHandler and the options
            // to the ServiceCollection plus options validation.
            // This would lead to duplicated registrations and incorrect behavior when
            // validating options (multiple validations and multiple error messages when validations fail).
            return httpClientBuilder
                .AddResilienceFallbackPolicy<TPolicyEventHandler>(optionsName)
                .AddResilienceRetryPolicy<TPolicyEventHandler>(optionsName)
                .AddResilienceCircuitBreakerPolicy<TPolicyEventHandler>(optionsName)
                .AddResilienceTimeoutPolicy<TPolicyEventHandler>(optionsName);
        }

        private static IHttpClientBuilder AddResilienceFallbackPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IFallbackPolicyEventHandler
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyEventHandler = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                if (!resilienceOptions.EnableFallbackPolicy)
                {
                    return new BlankHttpMessageHandler();
                }

                var retryPolicy = FallbackPolicyFactory.CreateFallbackPolicy(
                    httpClientBuilder.Name,
                    policyEventHandler);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }

        private static IHttpClientBuilder AddResilienceRetryPolicy<TPolicyEventHandler>(
            this IHttpClientBuilder httpClientBuilder,
            string optionsName)
            where TPolicyEventHandler : class, IRetryPolicyEventHandler
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyEventHandler = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.Retry,
                    policyEventHandler);
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
                var policyEventHandler = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(
                    httpClientBuilder.Name, 
                    resilienceOptions.CircuitBreaker,
                    policyEventHandler);
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
                var policyEventHandler = provider.GetRequiredService<TPolicyEventHandler>();
                var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
                var retryPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(
                    httpClientBuilder.Name,
                    resilienceOptions.Timeout,
                    policyEventHandler);
                return new PolicyHttpMessageHandler(retryPolicy);
            });
        }

        private class BlankHttpMessageHandler : DelegatingHandler
        {

        }
    }
}
