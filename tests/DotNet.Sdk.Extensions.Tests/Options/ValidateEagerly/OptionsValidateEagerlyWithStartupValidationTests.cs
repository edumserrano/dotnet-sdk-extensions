using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.StartupValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly
{
    // Note: the UseUrls method calls makes sure a random port is selected or it might fail
    // when running with other tests that also start a host because the port is already in use
    public class OptionsValidateEagerlyWithStartupValidationTests
    {
        /// <summary>
        /// This tests that the <see cref="IHost"/> will throw an exception when starting if the options
        /// validation fails.
        /// </summary>
        [Fact]
        public async Task ServerFailsToStartIfOptionsValidationFails()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls("http://*:0;https://*:0") 
                        .UseStartup<StartupMyOptions3ValidateEargerly>();
                })
                .Build();
            var validationException = await Should.ThrowAsync<OptionsValidationException>(host.StartAsync());
            validationException.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// This tests that the <see cref="IHost"/> will start as expected if the options validation succeeds.
        /// The validation is checking that the MyOptions3.SomeOption value needs to be > 1.
        /// </summary>
        [Fact]
        public void ServerStartsIfOptionsValidationSucceeds()
        {
            using var host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var memoryConfigurationSource = new MemoryConfigurationSource
                    {
                        InitialData = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("SomeOption", "2")
                            }
                    };
                    builder.Add(memoryConfigurationSource);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls("http://*:0;https://*:0")
                        .UseStartup<StartupMyOptions3ValidateEargerly>();
                })
                .Build();
            Should.NotThrow(async () => await host.StartAsync());
        }
    }
}
