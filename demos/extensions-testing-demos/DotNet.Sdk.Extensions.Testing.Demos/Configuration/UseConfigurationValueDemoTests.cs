using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Configuration;
using DotNet.Sdk.Extensions.Testing.Demos.Configuration.Auxiliary;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    /*
     * Shows how to use the IWebHostBuilder.SetConfigurationValue
     * to provide configuration values for integration tests.
     *
     */
    public class UseConfigurationValueDemoTests : IClassFixture<OverrideConfigurationWebApplicationFactory>
    {
        private readonly OverrideConfigurationWebApplicationFactory _webApplicationFactory;

        public UseConfigurationValueDemoTests(OverrideConfigurationWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }
        
        [Fact]
        public async Task UseConfigurationValueDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.SetConfigurationValue(key: "Option1", value: "value-from-test");
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/options");
            var firstMessage = await response.Content.ReadAsStringAsync();
            firstMessage.ShouldBe("value-from-test");
        }
    }
}
