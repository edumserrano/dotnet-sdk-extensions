using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions
{
    public static class CircuitBreakerOptionsExtensions
    {
        public static OptionsBuilder<CircuitBreakerOptions> AddHttpClientCircuitBreakerOptions(
            this IServiceCollection services,
            string name)
        {
            return services
                .AddOptions<CircuitBreakerOptions>(name)
                .ValidateDataAnnotations();
        }

        public static CircuitBreakerOptions GetHttpClientCircuitBreakerOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CircuitBreakerOptions>>();
            var options = optionsMonitor.Get(name);
            return options;
        }
    }
}

