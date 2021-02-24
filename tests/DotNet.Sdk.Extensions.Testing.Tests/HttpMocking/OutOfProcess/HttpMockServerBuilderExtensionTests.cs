using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess
{
    public class HttpMockServerBuilderExtensionTests
    {
        /// <summary>
        /// Tests that there is no default value set for the configuration key "Logging:LogLevel:Default".
        /// This is important because it serves as a control test for the following tests for the
        /// <see cref="HttpMockServerBuilderExtensions.UseDefaultLogLevel"/> extension method.
        /// </summary>
        [Fact]
        public async Task UseDefaultLogLevelControlTest()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

            var configuration = mock.Host!.Services.GetRequiredService<IConfiguration>();
            configuration["Logging:LogLevel:Default"].ShouldBe(null);
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockServerBuilderExtensions.UseDefaultLogLevel"/>
        /// sets the default log level on the <see cref="IConfiguration"/>.
        /// </summary>
        [Fact]
        public async Task UseDefaultLogLevel()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .Build();
            var urls = await mock.StartAsync();

            var configuration = mock.Host!.Services.GetRequiredService<IConfiguration>();
            configuration["Logging:LogLevel:Default"].ShouldBe("Critical");
        }
    }
}
