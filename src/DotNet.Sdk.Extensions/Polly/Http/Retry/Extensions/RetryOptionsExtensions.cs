using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions
{
    public static class RetryOptionsExtensions
    {
        public static OptionsBuilder<RetryOptions> AddHttpClientRetryOptions(
            this IServiceCollection services,
            string name)
        {
            return services
                .AddOptions<RetryOptions>(name: name)
                .ValidateDataAnnotations();
        }

        public static RetryOptions GetHttpClientRetryOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RetryOptions>>();
            var options = optionsMonitor.Get(name);
            return options;
        }
    }
}

