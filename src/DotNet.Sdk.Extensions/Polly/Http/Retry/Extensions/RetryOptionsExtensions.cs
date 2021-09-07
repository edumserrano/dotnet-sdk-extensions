using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="RetryOptions"/>.
    /// </summary>
    public static class RetryOptionsExtensions
    {
        /// <summary>
        /// Adds an instance of <see cref="RetryOptions"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// Same as doing services.AddOptions for a type RetryOptions with a name where services is of type <see cref="IServiceCollection"/>.
        /// This is an alias method to make it easier to add the options type required by the retry policy.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add the options to.</param>
        /// <param name="name">The name of the options.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for chaining.</returns>
        public static OptionsBuilder<RetryOptions> AddHttpClientRetryOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<RetryOptions>(name: name);
        }

        /// <summary>
        /// Retrieves the named <see cref="RetryOptions"/> options from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
        /// <param name="name">The name of the options.</param>
        /// <returns>An instance of <see cref="RetryOptions"/>.</returns>
        public static RetryOptions GetHttpClientRetryOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<RetryOptions>>();
            return optionsMonitor.Get(name);
        }
    }
}

