using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Options
{
    public static class OptionsBuilderExtensions
    {
        /// <summary>
        /// Binds configuration values to an options type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance containing values to bind to the options type.</param>
        /// <returns>The <see cref="OptionsBuilder&lt;T&gt;"/> for chaining.</returns>
        public static OptionsBuilder<T> AddOptions<T>(
            this IServiceCollection services,
            IConfiguration configuration) where T : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddOptions<T>()
                .Bind(configuration);
        }

        /// <summary>
        /// Binds configuration values from a section to an options type.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance containing values to bind to the options type.</param>
        /// <param name="sectionName">The name of the section from <see cref="IConfiguration"/> where values will be binded from.</param>
        /// <returns>The <see cref="OptionsBuilder&lt;T&gt;"/> for chaining.</returns>
        public static OptionsBuilder<T> AddOptions<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName) where T : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return services
                .AddOptions<T>()
                .Bind(configuration.GetSection(sectionName));
        }

        /// <summary>
        /// Allows resolving the type of the options added to the <see cref="IServiceCollection"/> instead of having to resolve
        /// one of the Options interfaces: <see cref="IOptions&lt;T&gt;"/>, <see cref="IOptionsSnapshot&lt;T&gt;"/> or <see cref="IOptionsMonitor&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// You first need to have configured an options of type T on the <see cref="IServiceCollection"/>.
        /// One way of doing this is by using:
        ///   <see cref="AddOptions&lt;T&gt;(IServiceCollection,IConfiguration)"/> or
        ///   <see cref="AddOptions&lt;T&gt;(IServiceCollection,IConfiguration,string)"/>
        /// </remarks>
        /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
        /// <param name="optionsBuilder">The <see cref="OptionsBuilder&lt;T&gt;"/> to add the options value to.</param>
        /// <returns>The <see cref="OptionsBuilder&lt;T&gt;"/> for chaining.</returns>
        public static OptionsBuilder<T> AddOptionsValue<T>(this OptionsBuilder<T> optionsBuilder) where T : class, new()
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            optionsBuilder.Services.AddOptionsValue<T>();
            return optionsBuilder;
        }

        /// <summary>
        /// Allows resolving the type of the options added to the <see cref="IServiceCollection"/> instead of having to resolve
        /// one of the Options interfaces: <see cref="IOptions&lt;T&gt;"/>, <see cref="IOptionsSnapshot&lt;T&gt;"/> or <see cref="IOptionsMonitor&lt;T&gt;"/>.
        /// </summary>
        /// <remarks>
        /// You first need to have configured an options of type T on the <see cref="IServiceCollection"/>.
        /// One way of doing this is by using:
        ///   <see cref="AddOptions&lt;T&gt;(IServiceCollection,IConfiguration)"/> or
        ///   <see cref="AddOptions&lt;T&gt;(IServiceCollection,IConfiguration,string)"/>
        /// </remarks>
        /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the options value to.</param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddOptionsValue<T>(this IServiceCollection services) where T : class, new()
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptions<T>>();
                return options.Value;
            });
            return services;
        }

        /// <summary>
        /// Force options validation at application startup.
        /// </summary>
        /// <remarks>
        /// At the moment there is no built-in mechanism to force eager validation on options so I've followed
        /// the advice from https://github.com/dotnet/extensions/issues/459
        /// </remarks>
        /// <typeparam name="T">The <see cref="Type"/>  of the options to be eagerly validated.</typeparam>
        /// <param name="optionsBuilder">The <see cref="OptionsBuilder&lt;T&gt;"/> for the options to add eager validation to.</param>
        /// <returns>The <see cref="OptionsBuilder&lt;T&gt;"/> for chaining.</returns>
        public static OptionsBuilder<T> ValidateEagerly<T>(this OptionsBuilder<T> optionsBuilder) where T : class
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            optionsBuilder.Services.AddTransient<IStartupFilter, StartupOptionsValidation<T>>();
            return optionsBuilder;
        }
    }
}
