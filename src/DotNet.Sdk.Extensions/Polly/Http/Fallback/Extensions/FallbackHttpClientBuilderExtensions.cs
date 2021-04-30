using DotNet.Sdk.Extensions.Polly.Http.Fallback.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions
{
    public static class FallbackHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddFallbackPolicyCore<DefaultFallbackPolicyConfiguration>();
        }
        
        public static IHttpClientBuilder AddFallbackPolicy<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyEventHandler : class, IFallbackPolicyConfiguration
        {
            return httpClientBuilder.AddFallbackPolicyCore<TPolicyEventHandler>();
        }

        private static IHttpClientBuilder AddFallbackPolicyCore<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyEventHandler : class, IFallbackPolicyConfiguration
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
