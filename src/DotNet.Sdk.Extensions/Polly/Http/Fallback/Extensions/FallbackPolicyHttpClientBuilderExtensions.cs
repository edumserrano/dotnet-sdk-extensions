using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions
{
    public static class FallbackPolicyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddFallbackPolicyCore<DefaultFallbackPolicyEventHandler>();
        }
        
        public static IHttpClientBuilder AddFallbackPolicy<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyEventHandler : class, IFallbackPolicyEventHandler
        {
            return httpClientBuilder.AddFallbackPolicyCore<TPolicyEventHandler>();
        }

        private static IHttpClientBuilder AddFallbackPolicyCore<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyEventHandler : class, IFallbackPolicyEventHandler
        {
            var httpClientName = httpClientBuilder.Name;
            httpClientBuilder.Services.AddSingleton<TPolicyEventHandler>();

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyEventHandler>();
                var fallbackPolicy = FallbackPolicyFactory.CreateFallbackPolicy(httpClientName, policyConfiguration);
                return new PolicyHttpMessageHandler(fallbackPolicy);
            });
        }
    }
}
