using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking
{
    /*
     * The http clients used by the startup class Startup_HttpResponseMocking all try to do a GET request
     * to a domain that does not exist and therefore should always fail but we mock the http message handler
     * for each of the http clients and make it return an OK status.
     *
     */
    public class HttpResponseMockingDemoTests : IClassFixture<HttpResponseMockingWebApplicationFactory>
    {
        private readonly HttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public HttpResponseMockingDemoTests(HttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }
        
        [Fact]
        public async Task MockBasicHttpClientDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureTestServices(services =>
                        {
                            // inject mocks for any other services
                        })
                        .UseHttpMocks(handlers =>
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

        [Fact]
        public async Task MockNamedHttpClientDemoTest()
        {
            var httpClientName = "my_named_client";
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureTestServices(services =>
                        {
                            // inject mocks for any other services
                        })
                        .UseHttpMocks(handlers =>
                        {
                            handlers.MockHttpResponse(httpResponseMessageBuilder =>
                            {
                                httpResponseMessageBuilder
                                    .ForNamedClient(httpClientName)
                                    .RespondWith(httpRequestMessage =>
                                    {
                                        return new HttpResponseMessage(HttpStatusCode.OK);
                                    });
                            });
                        });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/named-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe($"Named http client ({httpClientName}) returned: True");
        }


        [Fact]
        public async Task MockTypedHttpClientDemoTest()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureTestServices(services =>
                        {
                            // inject mocks for any other services
                        })
                        .UseHttpMocks(handlers =>
                        {
                            handlers.MockHttpResponse(httpResponseMessageBuilder =>
                            {
                                httpResponseMessageBuilder
                                    .ForTypedClient<IMyApiClient>()
                                    .RespondWith(httpRequestMessage =>
                                    {
                                        return new HttpResponseMessage(HttpStatusCode.OK);
                                    });
                            });
                        });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("IMyApiClient typed http client returned: True");
        }
    }
}
