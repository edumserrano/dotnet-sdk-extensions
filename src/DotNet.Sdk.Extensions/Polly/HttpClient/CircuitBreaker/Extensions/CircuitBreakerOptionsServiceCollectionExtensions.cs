using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions
{
    public static class CircuitBreakerOptionsServiceCollectionExtensions
    {
        public static OptionsBuilder<CircuitBreakerOptions> AddHttpClientCircuitBreakerOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<CircuitBreakerOptions>(name: name);
        }
    }
}

