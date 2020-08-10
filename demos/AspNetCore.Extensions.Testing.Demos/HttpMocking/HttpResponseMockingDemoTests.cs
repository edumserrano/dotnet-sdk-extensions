using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Extensions.Testing.Demos.TestApp.DemoStartups.HttpMocking;
using AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace AspNetCore.Extensions.Testing.Demos.HttpMocking
{
    /*
     * The http clients used by the startup class Startup_HttpResponseMocking all try to do a GET request
     * to a domain that does not exist and therefore should always fail but we mock the http message handler
     * for each of the http clients and make it return an OK status.
     *
     */
    public class HttpResponseMockingDemoTests : IClassFixture<WebApplicationFactory<Startup_HttpResponseMocking>>
    {
        private readonly WebApplicationFactory<Startup_HttpResponseMocking> _webApplicationFactory;

        public HttpResponseMockingDemoTests(WebApplicationFactory<Startup_HttpResponseMocking> webApplicationFactory)
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
                    builder.UseStartup<Startup_HttpResponseMocking>();
                });
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
                        .UseHttpMocks(httpMockBuilder =>
                        {
                            httpMockBuilder.MockHttpResponse(responseBuilder =>
                            {
                                responseBuilder
                                    .ForTypedClient<IMyApiClient>()
                                    .RespondWith(httpRequestMessage =>
                                    {
                                        return new HttpResponseMessage(HttpStatusCode.OK);
                                    });
                            });
                        });
                }).CreateClient();

            var response = await httpClient.GetAsync("/typed-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("IMyApiClient typed http client returned: True");
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
                        .UseHttpMocks(httpMessageHandlersBuilder =>
                        {
                            httpMessageHandlersBuilder.MockHttpResponse(mockBuilder =>
                            {
                                mockBuilder
                                    .ForNamedClient(httpClientName)
                                    .RespondWith(httpRequestMessage =>
                                    {
                                        return new HttpResponseMessage(HttpStatusCode.OK);
                                    });
                            });
                        });
                }).CreateClient();

            var response = await httpClient.GetAsync("/named-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe($"Named http client ({httpClientName}) returned: True");
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
                        .UseHttpMocks(httpMessageHandlersBuilder =>
                        {
                            httpMessageHandlersBuilder.MockHttpResponse(mockBuilder =>
                            {
                                mockBuilder
                                    .ForBasicClient()
                                    .RespondWith(httpRequestMessage =>
                                    {
                                        return new HttpResponseMessage(HttpStatusCode.OK);
                                    });
                            });
                        });
                }).CreateClient();

            var response = await httpClient.GetAsync("/basic-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("Basic http client returned: True");
        }
    }
}
