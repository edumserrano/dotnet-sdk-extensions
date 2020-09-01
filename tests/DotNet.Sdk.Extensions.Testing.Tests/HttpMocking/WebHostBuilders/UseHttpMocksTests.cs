using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders.Auxiliar;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders
{
    public class UseHttpMocksTests : IClassFixture<HttpResponseMockingWebApplicationFactory>
    {
        private readonly HttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public UseHttpMocksTests(HttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> returns the defined
        /// mock for a basic http client mock.
        /// This test uses the <see cref="HttpMessageHandlers.MockHttpResponse(Action{HttpResponseMessageMockDescriptorBuilder})"/> method.
        /// </summary>
        [Fact]
        public async Task BasicClientSimpleCase()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForBasicClient()
                                .RespondWith(httpRequestMessage =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.OK);
                                });
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/basic-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("Basic http client returned: True");
        }
    }
}
