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
    public class DisableLogsTests
    {
        /// <summary>
        /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
        /// This is important because it serves as a control test for the following tests for the
        /// <see cref="TestConfigurationHostBuilderExtensions.SetDefaultLogLevel"/> extension method.
        /// </summary>
        [Fact]
        public void HostDefaultLogLevelControlTest()
        {
            var host = Host
                .CreateDefaultBuilder()                      
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(null);
        } 
        
        /// <summary>
        /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
        /// This is important because it serves as a control test for the following tests for the
        /// <see cref="TestConfigurationWebHostBuilderExtensions.SetDefaultLogLevel"/> extension method.
        /// </summary>
        [Fact]
        public void WebHostDefaultLogLevelControlTest()
        {
            var host = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
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
        /// Tests that the <see cref="TestConfigurationHostBuilderExtensions.SetDefaultLogLevel"/>
        /// sets the default log level on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void HostSetLogLevel()
        {
            var host = Host
                .CreateDefaultBuilder()
                .SetDefaultLogLevel(LogLevel.None)                                                                                                         
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(LogLevel.None.ToString());
        }
        
        /// <summary>
        /// Tests that the <see cref="TestConfigurationWebHostBuilderExtensions.SetDefaultLogLevel"/>
        /// sets the default log level on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void WebHostSetLogLevel()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .SetDefaultLogLevel(LogLevel.None)
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            var logLevel = configuration.GetValue<string>(key: "Logging:LogLevel:Default");
            logLevel.ShouldBe(LogLevel.None.ToString());
        }
    }
}
