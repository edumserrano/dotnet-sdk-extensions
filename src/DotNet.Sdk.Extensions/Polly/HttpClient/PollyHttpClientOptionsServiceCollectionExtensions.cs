using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public static class PollyHttpClientOptionsServiceCollectionExtensions
    {
        public static OptionsBuilder<TimeoutOptions> AddHttpClientTimeoutOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<TimeoutOptions>(name: name);
        }
        
        public static OptionsBuilder<RetryOptions> AddHttpClientRetryOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<RetryOptions>(name: name);
        }

        public static OptionsBuilder<CircuitBreakerOptions> AddHttpClientCircuitBreakerOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<CircuitBreakerOptions>(name: name);
        }
    }
}

