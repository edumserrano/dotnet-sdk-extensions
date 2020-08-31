using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IWebHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">More appsettings' filenames if required.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var options = new TestConfigurationOptions();
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IWebHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="configureOptions">Options for the test appsettings.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">The appsettings' filenames.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            Action<TestConfigurationOptions> configureOptions,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new TestConfigurationOptions();
            configureOptions(options);
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        private static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            TestConfigurationOptions options,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (options == null) throw new ArgumentNullException(nameof(options)); 
            if (string.IsNullOrWhiteSpace(appSettingsFilename)) throw new ArgumentException("Cannot be null or white space.", nameof(appSettingsFilename));
            if (otherAppsettingsFilenames == null) throw new ArgumentNullException(nameof(otherAppsettingsFilenames));
            if (otherAppsettingsFilenames.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Cannot have an element that is null or white space.", nameof(otherAppsettingsFilenames));

            var projectDir = options.IsRelative 
                ? Path.Combine(Directory.GetCurrentDirectory(), options.AppSettingsDir) 
                : options.AppSettingsDir;
            return builder.ConfigureAppConfiguration((context, config) =>
            {
                /*
                 * Remove existing json sources. Without doing this the configuration files loaded
                 * normally during the app might interfere with the tests.
                 */
                config.Sources
                    .OfType<JsonConfigurationSource>()
                    .ToList()
                    .ForEach(source => config.Sources.Remove(source));
                var appsettingsFilenames = new[] { appSettingsFilename }.Concat(otherAppsettingsFilenames);
                foreach (var appSettingFilename in appsettingsFilenames)
                {
                    var configPath = Path.Combine(projectDir, appSettingFilename);
                    config.AddJsonFile(configPath, optional: false);
                }
            });
        }
    }
}
