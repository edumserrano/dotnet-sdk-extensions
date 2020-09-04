using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders.Auxiliar;
using Microsoft.AspNetCore.Hosting;
using NSubstitute.Core;
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
        /// Validates arguments for the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> extension method.
        /// </summary>
        [Fact]
        public void ValidateArguments()
        {
            var webHostBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() => HttpMockingWebHostBuilderExtensions.UseHttpMocks(null!, handlers => { }));
            webHostBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'webHostBuilder')");
            var configureArgumentNullException = Should.Throw<ArgumentNullException>(() => HttpMockingWebHostBuilderExtensions.UseHttpMocks(new WebHostBuilder(), null!));
            configureArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'configure')");
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
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("my-named-client")
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
            message.ShouldBe("Named http client (my-named-client) returned: True");
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

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> returns the defined
        /// mock for a typed http client with custom name mock.
        /// </summary>
        [Fact]
        public async Task TypedHttpClientWithCustomName()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForTypedClient<MyApiClient>("my-typed-client")
                                .RespondWith(httpRequestMessage =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.OK);
                                });
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client-with-custom-name");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("MyApiClient typed http client with custom name my-typed-client returned: True");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/> returns the defined
        /// mock for a typed http client with custom name mock.
        /// This test targets a slightly different way of registering a typed http client with custom than
        /// when compared with the <see cref="TypedHttpClientWithCustomName"/> test.
        /// </summary>
        [Fact]
        public async Task TypedHttpClientWithCustomName2()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForTypedClient<MyApiClient>("my-typed-client-2")
                                .RespondWith(httpRequestMessage =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.OK);
                                });
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client-with-custom-name-2");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("MyApiClient typed http client with custom name my-typed-client-2 returned: True");
        }
    }
}
