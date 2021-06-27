using System;
using System.IO;
using System.Linq;
using DotNet.Sdk.Extensions.Testing.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.Configuration
{
    [Trait("Category", XUnitCategories.Configuration)]
    public class AddTestConfigurationHostTests
    {
        /// <summary>
        /// Tests that <see cref="Host.CreateDefaultBuilder()"/> adds two <see cref="JsonConfigurationProvider"/>
        /// to the app configuration.
        /// This test serves as a control test because all the tests use the <see cref="WebHost.CreateDefaultBuilder()"/> as a way
        /// to setup a <see cref="IWebHost"/> with several <see cref="ConfigurationProvider"/> and at least two <see cref="JsonConfigurationProvider"/>.
        /// If this changes in the future then I could start having false positives on the other tests.
        /// </summary>
        [Fact]
        public void ControlTest()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers.OfType<JsonConfigurationProvider>();
            jsonConfigurationProviders.Count().ShouldBe(2);
        }

        public static TheoryData<IHostBuilder, string, string[], Type, string> ValidateArguments1Data =>
            new TheoryData<IHostBuilder, string, string[], Type, string>
            {
                { null!, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'builder')" },
                { new HostBuilder(), null!, Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new HostBuilder(), string.Empty, Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new HostBuilder(), " ", Array.Empty<string>(), typeof(ArgumentException), "Cannot be null or white space. (Parameter 'appSettingsFilename')" },
                { new HostBuilder(), "some-appsettings", null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'otherAppsettingsFilenames')" },
                { new HostBuilder(), "some-appsettings", new[] { "" }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
                { new HostBuilder(), "some-appsettings", new[] { " " }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
                { new HostBuilder(), "some-appsettings", new[] { "something","" }, typeof(ArgumentException), "Cannot have an element that is null or white space. (Parameter 'otherAppsettingsFilenames')" },
            };

        /// <summary>
        /// Validates arguments for the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, string, string[])"/>
        /// extension method
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Theory]
        [MemberData(nameof(ValidateArguments1Data))]
        public void ValidateArguments1(
            IHostBuilder hostBuilder,
            string appSettingsFilename,
            string[] otherAppSettingsFilenames,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(() =>
            {
                hostBuilder.AddTestAppSettings(appSettingsFilename, otherAppSettingsFilenames);
            }, exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// No need to test more scenarios because they're covered by <see cref="ValidateArguments1"/>
        /// </summary>
        public static TheoryData<IHostBuilder, Action<TestConfigurationOptions>, string, string[], Type, string> ValidateArguments2Data =>
            new TheoryData<IHostBuilder, Action<TestConfigurationOptions>, string, string[], Type, string>
            {
                { null!, options => { }, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'builder')" },
                { new HostBuilder(), null!, "some-appsettings", Array.Empty<string>(), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" }
            };

        /// <summary>
        /// Validates arguments for the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IWebHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// extension method
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Theory]
        [MemberData(nameof(ValidateArguments2Data))]
        public void ValidateArguments2(
            IHostBuilder hostBuilder,
            Action<TestConfigurationOptions> configureOptions,
            string appSettingsFilename,
            string[] otherAppSettingsFilenames,
            Type exceptionType,
            string exceptionMessage)
        {
            var exception = Should.Throw(() =>
            {
                hostBuilder.AddTestAppSettings(configureOptions, appSettingsFilename, otherAppSettingsFilenames);
            }, exceptionType);
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, string, string[])"/>
        /// with a single appsettings file results in a <see cref="ConfigurationRoot"/> which contains only the provided file.
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Fact]
        public void SingleFile()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .AddTestAppSettings("appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, string, string[])"/>
        /// with a multiple appsettings file results in a <see cref="ConfigurationRoot"/> which contains the provided files in the correct order.
        /// The test appsettings are loaded from the default directory: AppSettings.
        /// </summary>
        [Fact]
        public void MultipleFiles()
        {
            var host = Host
                .CreateDefaultBuilder()
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(3);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
            jsonConfigurationProviders[1].Source.Path.ShouldBe("appsettings.test2.json");
            jsonConfigurationProviders[2].Source.Path.ShouldBe("appsettings.test3.json");
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// allows loading files from a specific directory other than the default AppSettings directory.
        /// This tests using a relative directory which is the default on the <see cref="TestConfigurationOptions"/>.
        /// </summary>
        [Fact]
        public void SelectDirRelative()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .AddTestAppSettings(options => options.AppSettingsDir = "Configuration", "appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, Action{TestConfigurationOptions} , string, string[])"/>
        /// allows loading files from a specific directory other than the default AppSettings directory.
        /// This tests using an absolute directory.
        /// </summary>
        [Fact]
        public void SelectDirAbsolute()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .AddTestAppSettings(options =>
                {
                    options.AppSettingsDir = Path.Combine(Directory.GetCurrentDirectory(), "Configuration");
                    options.IsRelative = false;
                }, "appsettings.test.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var jsonConfigurationProviders = configuration.Providers
                .OfType<JsonConfigurationProvider>()
                .ToList();
            jsonConfigurationProviders.Count().ShouldBe(1);
            jsonConfigurationProviders[0].Source.Path.ShouldBe("appsettings.test.json");
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.AddTestAppSettings(IHostBuilder, string, string[])"/>
        /// preserves the expected order for configuration sources and therefore the expected loading configuration behavior.
        /// Meaning that configuration is taken from command line first, then environment variables, then appsettings files.
        /// </summary>
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // The default builder will add an EnvironmentVariablesConfigurationProvider.
                    // For this test I also need to have a CommandLineConfigurationProvider so the next line takes care of that.
                    builder.AddCommandLine(Array.Empty<string>());
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[1].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[2].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[3].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[4].ShouldBeOfType<EnvironmentVariablesConfigurationProvider>();
            configurationProviders[5].ShouldBeOfType<CommandLineConfigurationProvider>();
        }
        /// <summary>
        /// Similar to <see cref="PreservesExpectedConfigurationSourcesOrder"/> but tests when no <see cref="JsonConfigurationSource"/>
        /// exists. In this case the test appsettings should still be added before the <see cref="EnvironmentVariablesConfigurationSource"/>
        /// and if none exists, before the <see cref="CommandLineConfigurationSource"/>.
        /// </summary>
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder2()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // The default builder will add an EnvironmentVariablesConfigurationProvider.
                    // For this test I also need to have a CommandLineConfigurationProvider so the next line takes care of that.
                    builder.AddCommandLine(Array.Empty<string>());
                    builder.Sources
                        .OfType<JsonConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[1].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[2].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[3].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[4].ShouldBeOfType<EnvironmentVariablesConfigurationProvider>();
            configurationProviders[5].ShouldBeOfType<CommandLineConfigurationProvider>();
        }

        /// <summary>
        /// Similar to <see cref="PreservesExpectedConfigurationSourcesOrder"/> but tests when no <see cref="JsonConfigurationSource"/>
        /// and no <see cref="EnvironmentVariablesConfigurationSource"/> exist. In this case the test appsettings should still be added
        /// before the the <see cref="CommandLineConfigurationSource"/>.
        /// </summary>
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder3()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // The default builder will add an EnvironmentVariablesConfigurationProvider.
                    // For this test I also need to have a CommandLineConfigurationProvider so the next line takes care of that.
                    builder.AddCommandLine(Array.Empty<string>());
                    builder.Sources
                        .OfType<JsonConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                    builder.Sources
                        .OfType<EnvironmentVariablesConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[1].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[2].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[3].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[4].ShouldBeOfType<CommandLineConfigurationProvider>();
        }

        /// <summary>
        /// Similar to <see cref="PreservesExpectedConfigurationSourcesOrder"/> but tests when no <see cref="JsonConfigurationSource"/>,
        /// no <see cref="EnvironmentVariablesConfigurationSource"/> and no <see cref="CommandLineConfigurationSource"/> exist.
        /// In this case the test appsettings should be added at the end of the list of configuration sources.
        /// </summary>
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder4()
        {
            var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.Sources
                        .OfType<JsonConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                    builder.Sources
                        .OfType<EnvironmentVariablesConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[1].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[2].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[3].ShouldBeOfType<JsonConfigurationProvider>();
        }
        
        [Fact]
        public void PreservesExpectedConfigurationSourcesOrder5()
        {
            var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.Sources
                        .OfType<JsonConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                    builder.Sources
                        .OfType<EnvironmentVariablesConfigurationSource>()
                        .ToList()
                        .ForEach(source => builder.Sources.Remove(source));
                })
                .AddTestAppSettings("appsettings.test.json", "appsettings.test2.json", "appsettings.test3.json")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var configurationProviders = configuration.Providers.ToList();
            configurationProviders[1].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[2].ShouldBeOfType<JsonConfigurationProvider>();
            configurationProviders[3].ShouldBeOfType<JsonConfigurationProvider>();
        }
    }
}
