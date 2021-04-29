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
        
        public static IHttpClientBuilder AddFallbackPolicy<TPolicyConfiguration>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyConfiguration : class, IFallbackPolicyConfiguration
        {
            return httpClientBuilder.AddFallbackPolicyCore<TPolicyConfiguration>();
        }

        private static IHttpClientBuilder AddFallbackPolicyCore<TPolicyConfiguration>(this IHttpClientBuilder httpClientBuilder)
            where TPolicyConfiguration : class, IFallbackPolicyConfiguration
        {
            var httpClientName = httpClientBuilder.Name;
            httpClientBuilder.Services.AddSingleton<TPolicyConfiguration>();

            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var policyConfiguration = provider.GetRequiredService<TPolicyConfiguration>();
                var fallbackPolicy = FallbackPolicyFactory.CreateFallbackPolicy(httpClientName, policyConfiguration);
                return new PolicyHttpMessageHandler(fallbackPolicy);
            });
        }
    }
}
