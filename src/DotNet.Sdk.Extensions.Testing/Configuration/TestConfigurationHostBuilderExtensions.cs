using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    /// <summary>
    /// Provides extension methods to the <see cref="IHostBuilder"/> related with providing test configuration values via appsettings files.
    /// </summary>
    public static class TestConfigurationHostBuilderExtensions
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
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(key));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(value));
            }

            return hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(key, value)
                    }
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
    }
}
