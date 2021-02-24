using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.Demos.Configuration.Auxiliary;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    /*
     * Shows how to use the IWebHostBuilder.SetDefaultLogLevel
     * to set the default log level for integration tests.
     *
     */
    public class UseDefaultLogLevelDemoTests : IClassFixture<UseDefaultLogLevelWebApplicationFactory>
    {
        private readonly UseDefaultLogLevelWebApplicationFactory _webApplicationFactory;

        public UseDefaultLogLevelDemoTests(UseDefaultLogLevelWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }
        
        [Fact]
        public async Task UseDefaultLogLevelDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseDefaultLogLevel(LogLevel.None);
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/default-log-level");
            var firstMessage = await response.Content.ReadAsStringAsync();
            firstMessage.ShouldBe("None");
        }
    }
}
