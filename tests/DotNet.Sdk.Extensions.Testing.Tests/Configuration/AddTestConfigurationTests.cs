using System;
using System.IO;
using System.Linq;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.Tests.Auxiliary;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.Configuration
{
    public class AddTestConfigurationTests
    {
        /// <summary>
        /// Tests that <see cref="WebHost.CreateDefaultBuilder()"/> adds two <see cref="JsonConfigurationProvider"/>
        /// to the app configuration.
        /// This test serves as a control test because all the tests use the <see cref="WebHost.CreateDefaultBuilder()"/> as a way
        /// to setup a <see cref="IWebHost"/> with several <see cref="ConfigurationProvider"/> and at least two <see cref="JsonConfigurationProvider"/>.
        /// If this changes in the future then I could start having false positives on the other tests.
        /// </summary>
        [Fact]
        public void ControlTest()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers.OfType<JsonConfigurationProvider>();
            jsonConfigurationProviders.Count().ShouldBe(2);
        }

        public static TheoryData<IWebHostBuilder, string, string[], Type, string> ValidateArguments1Data =>
            new TheoryData<IWebHostBuilder, string, string[], Type, string>
            {
                { null!, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'builder')" },
                { new WebHostBuilder(), null!, Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new WebHostBuilder(), string.Empty, Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new WebHostBuilder(), " ", Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new WebHostBuilder(), "some-appsettings", null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'otherAppsettingsFilenames')" },
                { new WebHostBuilder(), "some-appsettings", new[] { "" }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
                { new WebHostBuilder(), "some-appsettings", new[] { " " }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
                { new WebHostBuilder(), "some-appsettings", new[] { "something","" }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
            };

        /// <summary>
        /// Validates arguments for the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, string, string[])"/>
        /// extension method
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Theory]
        [MemberData(nameof(ValidateArguments1Data))]
        public void ValidateArguments1(
            IWebHostBuilder webHostBuilder,
            string appSettingsFilename,
            string[] otherAppSettingsFilenames,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(() =>
            {
                webHostBuilder.AddTestAppSettings(appSettingsFilename, otherAppSettingsFilenames);
            }, exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// No need to test more scenarios because they're covered by <see cref="ValidateArguments1"/>
        /// </summary>
        public static TheoryData<IWebHostBuilder, Action<TestConfigurationOptions>, string, string[], Type, string> ValidateArguments2Data =>
            new TheoryData<IWebHostBuilder, Action<TestConfigurationOptions>, string, string[], Type, string>
            {
                { null!, options => { }, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'builder')" },
                { new WebHostBuilder(), null!, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" }
            };

        /// <summary>
        /// Validates arguments for the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// extension method
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Theory]
        [MemberData(nameof(ValidateArguments2Data))]
        public void ValidateArguments2(
            IWebHostBuilder webHostBuilder,
            Action<TestConfigurationOptions> configureOptions,
            string appSettingsFilename,
            string[] otherAppSettingsFilenames,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(() =>
            {
                webHostBuilder.AddTestAppSettings(configureOptions, appSettingsFilename, otherAppSettingsFilenames);
            }, exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// Tests that the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, string, string[])"/>
        /// with a single appsettings file results in a <see cref="ConfigurationRoot"/> which contains only the provided file.
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Fact]
        public void SingleFile()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings("appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, string, string[])"/>
        /// with a multiple appsettings file results in a <see cref="ConfigurationRoot"/> which contains the provided files in the correct order.
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Fact]
        public void MultipleFiles()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(3);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
            jsonConfigurationProviders[1].Source.Path.ShouldBe("appsettings.test2.json");
            jsonConfigurationProviders[2].Source.Path.ShouldBe("appsettings.test3.json");
        }

        /// <summary>
        /// Tests that the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// allows loading files from a specific directory other than the default AppSettings directory.
        /// This tests using a relative directory which is the default on the <see cref="TestConfigurationOptions"/>.
        /// </summary>
        [Fact]
        public void SelectDirRelative()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings(options => options.AppSettingsDir = "Configuration", "appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// allows loading files from a specific directory other than the default AppSettings directory.
        /// This tests using an absolute directory.
        /// </summary>
        [Fact]
        public void SelectDirAbsolute()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings(options =>
                {
                    options.AppSettingsDir = Path.Combine(Directory.GetCurrentDirectory(), "Configuration");
                    options.IsRelative = false;
                }, "appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="Testing.Configuration.WebHostBuilderExtensions.AddTestAppSettings(IWebHostBuilder, string, string[])"/>
        /// preserves the expected order for configuration sources and therefore the expected loading configuration behavior.
        /// Meaning that configuration is taken from command line first, then environment variables, then appsettings files. For this to happen
        /// the <see cref="CommandLineConfigurationProvider"/> must be the last provider in <see cref="IConfiguration"/> and the
        /// <see cref="EnvironmentVariablesConfigurationProvider"/> the one before that.
        /// </summary>
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // The default builder will add an EnvironmentVariablesConfigurationProvider.
                    // For this test I also need to have a CommandLineConfigurationProvider so the next line takes care of that.
                    builder.AddCommandLine(Array.Empty<string>());
                })
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[^1].ShouldBeOfType<CommandLineConfigurationProvider>();
            configurationProviders[^2].ShouldBeOfType<EnvironmentVariablesConfigurationProvider>();
        }
    }
}
