using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions
{
    public static class RetryOptionsServiceCollectionExtensions
    {
        public static OptionsBuilder<RetryOptions> AddHttpClientRetryOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<RetryOptions>(name: name);
        }
    }
}

