using DotNet.Sdk.Extensions.Polly.HttpClient.DelegatingHandlers;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public static class PollyHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddCircuitBreakerHandler(
            this IHttpClientBuilder httpClientBuilder,
            string policyKey)
        {

            return httpClientBuilder
                .AddHttpMessageHandler(serviceProvider => //check circuit breaker state before even trying
                {
                    var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
                    var circuitBreakerPolicy = registry.Get<ICircuitBreakerPolicy>(policyKey);
                    return new CircuitBreakerCheckerHandler(circuitBreakerPolicy);
                })
                .AddPolicyHandlerFromRegistry(policyKey);
        }
    }
}
