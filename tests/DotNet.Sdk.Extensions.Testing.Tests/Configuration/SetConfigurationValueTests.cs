using System;
using DotNet.Sdk.Extensions.Testing.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.Configuration
{
    public class SetConfigurationValueTests
    {
        /// <summary>
        /// Validates arguments the <see cref="TestConfigurationHostBuilderExtensions.SetConfigurationValue"/>.
        /// </summary>
        [Theory]
        [InlineData(null,"value1", "Cannot be null or empty. (Parameter 'key')")]
        [InlineData("","value1", "Cannot be null or empty. (Parameter 'key')")]
        [InlineData("some-key",null, "Cannot be null or empty. (Parameter 'value')")]
        [InlineData("some-key","", "Cannot be null or empty. (Parameter 'value')")]
        public void HostValidateArguments(string key, string value, string exceptionMessage)
        {
            var exception = Should.Throw<ArgumentException>(() =>
            {
                return Host
                    .CreateDefaultBuilder()
                    .SetConfigurationValue(key: key, value: value);
            });
            exception.Message.ShouldBe(exceptionMessage);
        }
        
        /// <summary>
        /// Validates arguments the <see cref="TestConfigurationWebHostBuilderExtensions.SetConfigurationValue"/>.
        /// </summary>
        [Theory]
        [InlineData(null,"value1", "Cannot be null or empty. (Parameter 'key')")]
        [InlineData("","value1", "Cannot be null or empty. (Parameter 'key')")]
        [InlineData("some-key",null, "Cannot be null or empty. (Parameter 'value')")]
        [InlineData("some-key","", "Cannot be null or empty. (Parameter 'value')")]
        public void WebHostValidateArguments(string key, string value, string exceptionMessage)
        {
            var exception = Should.Throw<ArgumentException>(() =>
            {
                return WebHost
                    .CreateDefaultBuilder()
                    .Configure((context, applicationBuilder) =>
                    {
                        // this is required just to provide a configuration for the webhost
                        // or else it fails when calling webHostBuilder.Build()
                    })
                    .SetConfigurationValue(key: key, value: value);
            });
            exception.Message.ShouldBe(exceptionMessage);
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationHostBuilderExtensions.SetConfigurationValue"/>
        /// sets the configuration value on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void HostSetConfigurationValue()
        {
            var host = Host
                .CreateDefaultBuilder()
                .SetConfigurationValue(key:"SomeValue1",value:"value-1") 
                .SetConfigurationValue(key:"SomeValue2",value:"value-2") 
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            configuration.GetValue<string>(key: "SomeValue1").ShouldBe("value-1");
            configuration.GetValue<string>(key: "SomeValue2").ShouldBe("value-2");
        }

        /// <summary>
        /// Tests that the <see cref="TestConfigurationWebHostBuilderExtensions.SetConfigurationValue"/>
        /// sets the configuration value  on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public void WebHostSetConfigurationValue()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .SetConfigurationValue(key: "SomeValue1", value: "value-1")
                .SetConfigurationValue(key: "SomeValue2", value: "value-2")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            configuration.GetValue<string>(key: "SomeValue1").ShouldBe("value-1");
            configuration.GetValue<string>(key: "SomeValue2").ShouldBe("value-2");
        }

        /// <summary>
        /// When SetConfigurationValue is called multiple times for the same key, the last value is what
        /// gets set.
        /// </summary>
        [Fact]
        public void SetConfigurationValueLastValueWins()
        {
            var host = Host
                .CreateDefaultBuilder()
                .SetConfigurationValue(key: "SomeValue1", value: "value-1")
                .SetConfigurationValue(key: "SomeValue1", value: "value-2")
                .Build();
            var configuration = (ConfigurationRoot)host.Services.GetRequiredService<IConfiguration>();
            configuration.GetValue<string>(key: "SomeValue1").ShouldBe("value-2");
        }

        /// <summary>
        /// Similar to <see cref="SetConfigurationValueLastValueWins"/>. The last value set wins, since
        /// appsettings are loaded first, the value set in the test will be the one present on the configuration.
        /// </summary>
        [Fact]
        public void OverrideAppsettingsWithSetConfigurationValue()
        {
            var webHost = WebHost
                .CreateDefaultBuilder()
                .Configure((context, applicationBuilder) =>
                {
                    // this is required just to provide a configuration for the webhost
                    // or else it fails when calling webHostBuilder.Build()
                })
                .AddTestAppSettings(options => options.AppSettingsDir = "Configuration", "appsettings.setconfigurationvalues.test.json")
                .SetConfigurationValue(key: "SomeValue1", value: "overriden-on-test")
                .Build();
            var configuration = (ConfigurationRoot)webHost.Services.GetRequiredService<IConfiguration>();
            configuration.GetValue<string>(key: "SomeValue1").ShouldBe("overriden-on-test");
        }
    }
}
