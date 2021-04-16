using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions
{
    public static class TimeoutOptionsServiceCollectionExtensions
    {
        public static OptionsBuilder<TimeoutOptions> AddHttpClientTimeoutOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<TimeoutOptions>(name: name);
        }
    }
}

