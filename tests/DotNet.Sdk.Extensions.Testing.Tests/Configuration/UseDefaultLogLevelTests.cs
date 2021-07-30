using DotNet.Sdk.Extensions.Testing.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.Configuration
{
    [Trait("Category", XUnitCategories.Configuration)]
    public class UseDefaultLogLevelTests
    {
        /// <summary>
        /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
        /// This is important because it serves as a control test for the following tests for the
        /// other tests here.
        /// </summary>
        [Fact]
        public void HostDefaultLogLevelControlTest()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(null);
        }

        /// <summary>
        /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
        /// This is important because it serves as a control test for the following tests for the
        /// <see cref="TestConfigurationBuilderExtensions.UseDefaultLogLevel(IHostBuilder,LogLevel)"/> extension method.
        /// </summary>
        [Fact]
        public void WebHostDefaultLogLevelControlTest()
        {
            using var host = WebHost
                .CreateDefaultBuilder()
                .Configure((_, _) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(null);
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.UseDefaultLogLevel"/>
        /// sets the default log level on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void HostSetLogLevel()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .UseDefaultLogLevel(LogLevel.None)
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(nameof(LogLevel.None));
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationBuilderExtensions.UseDefaultLogLevel(IHostBuilder,LogLevel)"/>
        /// sets the default log level on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void WebHostSetLogLevel()
        {
            using var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((_, _) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .UseDefaultLogLevel(LogLevel.None)
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(nameof(LogLevel.None));
        }
    }
}
