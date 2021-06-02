using System;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions
{
    public static class FallbackPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder)
        {
            Func<IServiceProvider, IFallbackPolicyEventHandler> eventHandlerFactory = _ => new DefaultFallbackPolicyEventHandler();
            return httpClientBuilder.AddFallbackPolicy(eventHandlerFactory);
        }

        public static IHttpClientBuilder AddFallbackPolicy<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyEventHandler : class, IFallbackPolicyEventHandler
        {
            httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
            Func<IServiceProvider, IFallbackPolicyEventHandler> eventHandlerFactory = provider => provider.GetRequiredService<TPolicyEventHandler>();
            return httpClientBuilder.AddFallbackPolicy(eventHandlerFactory);
        }
        
        public static IHttpClientBuilder AddFallbackPolicy(
            this IHttpClientBuilder httpClientBuilder,
            Func<IServiceProvider, IFallbackPolicyEventHandler> eventHandlerFactory)
        {
            var httpClientName = httpClientBuilder.Name;
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyEventHandler = eventHandlerFactory(provider);
                var fallbackPolicy = FallbackPolicyFactory.CreateFallbackPolicy(httpClientName, policyEventHandler);
                return new PolicyHttpMessageHandler(fallbackPolicy);
            });
        }
    }
}
