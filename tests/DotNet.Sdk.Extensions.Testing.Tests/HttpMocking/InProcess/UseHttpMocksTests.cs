using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.UseHttpMocks;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess
{
    public class UseHttpMocksTests : IClassFixture<HttpResponseMockingWebApplicationFactory>
    {
        private readonly HttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public UseHttpMocksTests(HttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// Validates arguments for the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> extension method.
        /// </summary>
        [Fact]
        public void ValidateArguments()
        {
            var webHostBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() => HttpMockingWebHostBuilderExtensions.UseHttpMocks(null!, handlers => { }));
            webHostBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'webHostBuilder')");
            var configureArgumentNullException = Should.Throw<ArgumentNullException>(() => new WebHostBuilder().UseHttpMocks((Action<HttpMessageHandlersReplacer>)null!));
            configureArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'configure')");
        }

        /// <summary>
        /// Validates arguments for the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, HttpResponseMessageMockDescriptorBuilder[])"/> extension method.
        /// </summary>
        [Fact]
        public void ValidateArguments2()
        {
            var webHostBuilderArgumentNullException = Should.Throw<ArgumentNullException>(() => HttpMockingWebHostBuilderExtensions.UseHttpMocks(null!, handlers => { }));
            webHostBuilderArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'webHostBuilder')");
            var configureArgumentNullException = Should.Throw<ArgumentNullException>(() => new WebHostBuilder().UseHttpMocks((HttpResponseMessageMockDescriptorBuilder[])null!));
            configureArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'httpResponseMessageMockDescriptorBuilders')");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> returns the defined
        /// mock for a basic http client mock.
        /// </summary>
        [Fact]
        public async Task BasicClient()
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
                                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/basic-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("Basic http client returned: True");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> returns the defined
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
                                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/named-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("Named http client (my-named-client) returned: True");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> returns the defined
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
                                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("MyApiClient typed http client returned: True");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> returns the defined
        /// mock for a typed http client with custom name mock.
        ///
        /// This tests typed clients with name registered on the Startup class such as:
        /// 'services.AddHttpClient<MyApiClient>("some name");'
        /// See <see cref="StartupHttpResponseMocking"/> for more information.
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
                                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client-with-custom-name");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("MyApiClient typed http client with custom name my-typed-client returned: True");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> returns the defined
        /// mock for a typed http client with custom name mock.
        /// This test targets a slightly different way of registering a typed http client with custom than
        /// when compared with the <see cref="TypedHttpClientWithCustomName"/> test.
        ///
        /// This tests typed clients with name registered on the Startup class such as:
        /// 'services.AddHttpClient("my-typed-client-2").AddTypedClient<MyApiClient>();'
        /// See <see cref="StartupHttpResponseMocking"/> for more information.
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
                                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
                        });
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/typed-client-with-custom-name-2");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("MyApiClient typed http client with custom name my-typed-client-2 returned: True");
        }
        
        /// <summary>
        /// Tests the <seealso cref="HttpMessageHandlersReplacer.MockHttpResponse(HttpResponseMessageMockDescriptorBuilder)"/> API where you
        /// can define the mocks before hand instead of being inline.
        /// </summary>
        [Fact]
        public async Task MockHttpResponseNonInlineMocking()
        {
            var httpResponseMock1 = new HttpResponseMessageMockDescriptorBuilder();
            httpResponseMock1
                    .ForBasicClient()
                    .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
            var httpResponseMock2 = new HttpResponseMessageMockDescriptorBuilder();
            httpResponseMock2
                .ForNamedClient("my-named-client")
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMock1);
                        handlers.MockHttpResponse(httpResponseMock2);
                    });
                })
                .CreateClient();

            var response = await httpClient.GetAsync("/basic-client");
            var message = await response.Content.ReadAsStringAsync();
            message.ShouldBe("Basic http client returned: True");

            var response2 = await httpClient.GetAsync("/named-client");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("Named http client (my-named-client) returned: True");
        }

        /// <summary>
        /// Tests the <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, HttpResponseMessageMockDescriptorBuilder[])"/> API where you
        /// can define the mocks before hand instead of being inline.
        /// </summary>
        [Fact]
        public async Task UseHttpMocksNonInlineMocking()
        {
            var httpResponseMock1 = new HttpResponseMessageMockDescriptorBuilder();
            httpResponseMock1
                .ForBasicClient()
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));
            var httpResponseMock2 = new HttpResponseMessageMockDescriptorBuilder();
            httpResponseMock2
                .ForNamedClient("my-named-client")
                .RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(httpResponseMock1, httpResponseMock2);
                })
                .CreateClient();

            var response1 = await httpClient.GetAsync("/basic-client");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("Basic http client returned: True");

            var response2 = await httpClient.GetAsync("/named-client");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("Named http client (my-named-client) returned: True");
        }
    }
}
