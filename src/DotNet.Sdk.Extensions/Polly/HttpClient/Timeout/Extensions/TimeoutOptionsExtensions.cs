using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions
{
    public static class TimeoutOptionsExtensions
    {
        public static OptionsBuilder<TimeoutOptions> AddHttpClientTimeoutOptions(
            this IServiceCollection services,
            string name)
        {
            return services
                .AddOptions<TimeoutOptions>(name: name)
                .ValidateDataAnnotations();
        }

        public static TimeoutOptions GetHttpClientTimeoutOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TimeoutOptions>>();
            var options = optionsMonitor.Get(name);
            return options;
        }
    }
}

