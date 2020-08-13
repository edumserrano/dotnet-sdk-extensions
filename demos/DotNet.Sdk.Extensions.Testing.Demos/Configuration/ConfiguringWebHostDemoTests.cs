using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.Demos.TestApp.DemoStartups.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    public class ConfiguringWebHostDemoTests : IClassFixture<WebApplicationFactory<Startup_ConfiguringWebHost>>
    {
        private readonly WebApplicationFactory<Startup_ConfiguringWebHost> _webApplicationFactory;

        public ConfiguringWebHostDemoTests(WebApplicationFactory<Startup_ConfiguringWebHost> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;

            /*
             * The line below is NOT part of the demo. You don't need to do it!
             * It's an artifact of having a single web app to test and wanting to test
             * different Startup classes.
             */
            _webApplicationFactory = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseStartup<Startup_ConfiguringWebHost>();
                });
        }

        [Fact]
        public async Task ConfiguringWebHostDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.AddTestConfiguration(options => options.AppSettingsDir = "Configuration/AppSettings", "appsettings.json", "appsettings.Default.json");
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
