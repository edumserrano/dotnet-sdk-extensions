using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.InProcess.Auxiliary;
using DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.InProcess
{
    /*
     * The http clients used by the startup class InProcessHttpResponseMockingStartup all try to do a GET request
     * to a domain that does not exist and therefore should always fail but we mock the http message handler
     * for each of the http clients and make it return an OK status.
     *
     */
    public class InProcessHttpResponseMockingDemoTests : IClassFixture<InProcessHttpResponseMockingWebApplicationFactory>
    {
        private readonly InProcessHttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public InProcessHttpResponseMockingDemoTests(InProcessHttpResponseMockingWebApplicationFactory webApplicationFactory)
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
            message.ShouldBe("ISomeApiClient typed http client returned: True");
        }

        /*
         * Similar to the previous demo tests but shows a different usage of the
         * HttpMessageHandlersReplacer.SimpleStartup where you specify the mock before hand
         * instead of inline.
         * 
         * This is not specific to basic or typed clients, the same will work for any kind of HttpClient.
         * 
         * There is no recommendation on inline vs non inline. You should use the option
         * that fits better your scenario/style.
         */
        [Fact]
        public async Task MockHttpResponseNonInlineMocking()
        {
            var myApiClientResponse1 = new HttpResponseMessageMockDescriptorBuilder();
            myApiClientResponse1.ForBasicClient()
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
            var myApiClientResponse2 = new HttpResponseMessageMockDescriptorBuilder();
            myApiClientResponse2
                .ForTypedClient<IMyApiClient>()
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));

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
                            handlers.MockHttpResponse(myApiClientResponse1);
                            handlers.MockHttpResponse(myApiClientResponse2);
                        });
                })
                .CreateClient();

            var response1 = await httpClient.GetAsync("/basic-client");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("Basic http client returned: True");

            var response2 = await httpClient.GetAsync("/typed-client");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("ISomeApiClient typed http client returned: True");
        }


        /*
         * Similar to the previous demo tests but shows a different usage of the
         * IWebHostBuilder.UseHttpMocks where you specify the mock before hand
         * instead of inline.
         * 
         * This is not specific to basic or typed clients, the same will work for any kind of HttpClient.
         *
         * There is no recommendation on inline vs non inline. You should use the option
         * that fits better your scenario/style.
         */
        [Fact]
        public async Task UseHttpMocksNonInlineMocking()
        {
            var myApiClientResponse1 = new HttpResponseMessageMockDescriptorBuilder();
            myApiClientResponse1.ForBasicClient()
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
            var myApiClientResponse2 = new HttpResponseMessageMockDescriptorBuilder();
            myApiClientResponse2
                .ForTypedClient<IMyApiClient>()
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureTestServices(services =>
                        {
                            // inject mocks for any other services
                        })
                        .UseHttpMocks(myApiClientResponse1, myApiClientResponse2);
                })
                .CreateClient();

            var response1 = await httpClient.GetAsync("/basic-client");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("Basic http client returned: True");

            var response2 = await httpClient.GetAsync("/typed-client");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("ISomeApiClient typed http client returned: True");
        }
    }
}
