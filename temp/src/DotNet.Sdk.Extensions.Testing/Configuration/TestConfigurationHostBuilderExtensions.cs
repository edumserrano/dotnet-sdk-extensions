using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    /// <summary>
    /// Provides extension methods to the <see cref="IHostBuilder"/> related with providing test configuration values via appsettings files.
    /// </summary>
    public static partial class TestConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds a value to the <see cref="IConfiguration"/> using a <see cref="MemoryConfigurationSource"/>.
        /// Allows overwriting specific configuration values when doing tests.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance.</param>
        /// <param name="key">The key of the configuration value.</param>
        /// <param name="value">The value to set on the configuration.</param>
        /// <returns>The <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseConfigurationValue(this IHostBuilder hostBuilder, string key, string value)
        {
            if (hostBuilder is null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(key));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(value));
            }

            return hostBuilder.ConfigureAppConfiguration((_, builder) =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(key, value),
                    },
                };
                builder.Add(memoryConfigurationSource);
            });
        }

        /// <summary>
        /// Sets the default log level for the application.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance.</param>
        /// <param name="logLevel">The default log level.</param>
        /// <returns>The <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseDefaultLogLevel(this IHostBuilder hostBuilder, LogLevel logLevel)
        {
            return hostBuilder.UseConfigurationValue("Logging:LogLevel:Default", $"{logLevel}");
        }

        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary>
        /// <remarks>
        /// It also makes sure that the expected loading configuration behavior is preserved by having the
        /// <see cref="CommandLineConfigurationSource"/> last and the <see cref="EnvironmentVariablesConfigurationSource"/>
        /// second to last in the <see cref="IConfigurationBuilder.Sources"/>.
        /// This way it will keep the loading configuration behavior of:
        /// configuration taken from command line first, then environment variables, then appsettings files.
        /// </remarks>
        /// <param name="builder">The <see cref="IHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">More appsettings' filenames if required.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddTestAppSettings(
            this IHostBuilder builder,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new TestConfigurationOptions();
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary>
        /// <remarks>
        /// It also makes sure that the expected loading configuration behavior is preserved by having the
        /// <see cref="CommandLineConfigurationSource"/> last and the <see cref="EnvironmentVariablesConfigurationSource"/>
        /// second to last in the <see cref="IConfigurationBuilder.Sources"/>.
        /// This way it will keep the loading configuration behavior of:
        /// configuration taken from command line first, then environment variables, then appsettings files.
        /// </remarks>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="configureOptions">Options for the test appsettings.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">The appsettings' filenames.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddTestAppSettings(
            this IHostBuilder builder,
            Action<TestConfigurationOptions> configureOptions,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var options = new TestConfigurationOptions();
            configureOptions(options);
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        private static IHostBuilder AddTestAppSettings(
            this IHostBuilder builder,
            TestConfigurationOptions options,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(appSettingsFilename))
            {
                throw new ArgumentException("Cannot be null or white space.", nameof(appSettingsFilename));
            }

            if (otherAppsettingsFilenames is null)
            {
                throw new ArgumentNullException(nameof(otherAppsettingsFilenames));
            }

            if (otherAppsettingsFilenames.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Cannot have an element that is null or white space.", nameof(otherAppsettingsFilenames));
            }

            return builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
            });
        }
    }
}
