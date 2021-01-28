using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.Demos.Configuration.Auxiliary;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    public class ConfiguringWebHostDemoTests : IClassFixture<ConfiguringWebHostWebApplicationFactory>
    {
        private readonly ConfiguringWebHostWebApplicationFactory _webApplicationFactory;

        public ConfiguringWebHostDemoTests(ConfiguringWebHostWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        [Fact]
        public async Task ConfiguringWebHostDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.AddTestAppSettings(options => options.AppSettingsDir = "Configuration/AppSettings", "appsettings.json", "appsettings.Default.json");
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/message-one");
            var firstMessage = await response.Content.ReadAsStringAsync();
            firstMessage.ShouldBe("Hi from message one in test appsettings.json");

            response = await httpClient.GetAsync("/message-two");
            var secondMessage = await response.Content.ReadAsStringAsync();
            secondMessage.ShouldBe("Hi from message two in test appsettings.Default.json");
        }
    }
}
