using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="TimeoutOptions"/>.
    /// </summary>
    public static class TimeoutOptionsExtensions
    {
        /// <summary>
        /// Adds an instance of <see cref="TimeoutOptions"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// Same as doing services.AddOptions for a type TimeoutOptions with a name where services is of type <see cref="IServiceCollection"/>.
        /// This is an alias method to make it easier to add the options type required by the timeout policy.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add the options to.</param>
        /// <param name="name">The name of the options.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for chaining.</returns>
        public static OptionsBuilder<TimeoutOptions> AddHttpClientTimeoutOptions(
            this IServiceCollection services,
            string name)
        {
            return services.AddOptions<TimeoutOptions>(name: name);
        }

        /// <summary>
        /// Retrieves the named <see cref="TimeoutOptions"/> options from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
        /// <param name="name">The name of the options.</param>
        /// <returns>An instance of <see cref="TimeoutOptions"/>.</returns>
        public static TimeoutOptions GetHttpClientTimeoutOptions(
            this IServiceProvider serviceProvider,
            string name)
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TimeoutOptions>>();
            return optionsMonitor.Get(name);
        }
    }
}
