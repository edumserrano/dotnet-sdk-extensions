using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders.Auxiliar;
using Microsoft.AspNetCore.TestHost;
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

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> returns the defined
        /// mock for a named http client mock.
        /// </summary>
        [Fact]
        public async Task NamedHttpClient()
        {
            var httpClientName = "my-named-client";
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseHttpMocks(handlers =>
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

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> returns the defined
        /// mock for a typed http client mock.
        /// </summary>
        [Fact]
        public async Task TypedHttpClient()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForTypedClient<MyApiClient>()
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
            message.ShouldBe("MyApiClient typed http client returned: True");
        }
    }
}
