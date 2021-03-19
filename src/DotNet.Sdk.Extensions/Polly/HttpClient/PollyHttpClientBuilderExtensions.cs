using DotNet.Sdk.Extensions.Polly.HttpClient.DelegatingHandlers;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public static class PollyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddCircuitBreakerCheckerHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey)
        {

            return httpClientBuilder.AddHttpMessageHandler(serviceProvider =>
            {
                var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
                var circuitBreakerPolicy = registry.Get<ICircuitBreakerPolicy>(policyKey);
                return new CircuitBreakerCheckerHandler(circuitBreakerPolicy);
            });
        }
    }
}
