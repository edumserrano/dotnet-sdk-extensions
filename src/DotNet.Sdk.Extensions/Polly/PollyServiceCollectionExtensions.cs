using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace DotNet.Sdk.Extensions.Polly
{
    /// <summary>
    /// Provides convenience extension methods to register <see cref="IPolicyRegistry{String}"/> and 
    /// <see cref="IReadOnlyPolicyRegistry{String}"/> in the service collection.
    /// </summary>
    public static class PollyServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the <see cref="IPolicyRegistry{String}"/> in the service collection with service types
        /// <see cref="IPolicyRegistry{String}"/>, and <see cref="IReadOnlyPolicyRegistry{String}"/>.
        /// </summary>
        /// <remarks>
        /// Implementation based on
        /// https://github.com/dotnet/aspnetcore/blob/b584eafb19af815a41928a011af675425c5f1a13/src/HttpClientFactory/Polly/src/DependencyInjection/PollyServiceCollectionExtensions.cs
        /// but extending it to take in the <see cref="IServiceProvider"/> so that you can setup the policy registry and consume data from
        /// the service provider such as configuration values.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureRegistry">Action to setup the <see cref="IPolicyRegistry{String}"/></param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddPolicyRegistry(
            this IServiceCollection services,
            Action<IServiceProvider, IPolicyRegistry<string>> configureRegistry)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            return services
                .AddSingleton<IPolicyRegistry<string>>(serviceProvider =>
                {
                    var registry = new PolicyRegistry();
                    configureRegistry(serviceProvider, registry);
                    return registry;
                })
                .AddSingleton<IReadOnlyPolicyRegistry<string>>(serviceProvider =>
                {
                    // makes sure the above factory method on the registration of the IPolicyRegistry<string>
                    // is executed. In other words it forces the configureRegistry action to be called
                    // regardless if you ask for an instance of IPolicyRegistry<string> or IReadOnlyPolicyRegistry<string>
                    return serviceProvider.GetRequiredService<IPolicyRegistry<string>>();
                });
        }
    }
}
