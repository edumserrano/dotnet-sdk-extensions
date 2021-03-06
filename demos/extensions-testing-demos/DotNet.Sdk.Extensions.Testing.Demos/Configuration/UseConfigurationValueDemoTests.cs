using System;
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
    public class UseConfigurationValueDemoTests : IClassFixture<UseConfigurationValueWebApplicationFactory>, IDisposable
    {
        private readonly UseConfigurationValueWebApplicationFactory _valueWebApplicationFactory;

        public UseConfigurationValueDemoTests(UseConfigurationValueWebApplicationFactory valueWebApplicationFactory)
        {
            _valueWebApplicationFactory = valueWebApplicationFactory;
        }
        
        [Fact]
        public async Task UseConfigurationValueDemoTest()
        {
            var httpClient = _valueWebApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseConfigurationValue(key: "Option1", value: "value-from-test");
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/options");
            var firstMessage = await response.Content.ReadAsStringAsync();
            firstMessage.ShouldBe("value-from-test");
        }

        public void Dispose()
        {
            _valueWebApplicationFactory.Dispose();
        }
    }
}
