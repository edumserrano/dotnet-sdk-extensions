using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.Auxiliary;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.ResponseBased
{
    /*
     * The OutOfProcessHttpResponseMockingStartup represents the real app that you want to do integration tests
     * on. This app has an HttpClient that must be configured to send requests to the HttpMockServer or else those outgoing
     * http requests would fail.
     *
     * The app we want to test, represented by OutOfProcessHttpResponseMockingStartup, has 3 endpoints:
     * 1) /users
     * 2) /admin
     * 3) /configuration
     *
     * Each of those endpoints has an HttpClient sending a GET request to the same path. Meaning the HttpMockServer
     * must be configured to be able to reply to GET requests on these 3 paths.
     *
     */
    public class OutOfProcessResponseBasedDemoTests : IClassFixture<OutOfProcessHttpResponseMockingWebApplicationFactory>, IDisposable
    {
        private readonly OutOfProcessHttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public OutOfProcessResponseBasedDemoTests(OutOfProcessHttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        [Fact]
        public async Task MockHttpResponse()
        {
            // First configure the HttpResponseMocks and the HttpMockServer
            var httpResponseMock = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/users"))
                .RespondWith((request, response) => response.StatusCode = StatusCodes.Status200OK)
                .Build();
            
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock)
                .Build();
            var urls = await httpMockServer.StartAsync();
            // by default the httpMockServer will be listening on a random free http port and a random free https port
            var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
            var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);

            // Now configure the WebApplicationFactory so the HttpClient you intend to test
            // sends requests to the HttpMockServer created
            // The way this is done depends entirely on how you configured your application and thus
            // the code below might not apply for your use case. In the example below, the app will read
            // the base address that the HttpClient will use from the options class named HttpClientsOptions.
            // As such we make sure that its value is set to one of the HttpMockServer's listening URL
            var httpClientsOptions = new HttpClientsOptions
            {
                NamedClientBaseAddress = httpUrl
            };
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IOptions<HttpClientsOptions>>(Options.Create(httpClientsOptions));
                        // inject mocks for any other services
                    });
                })
                .CreateClient();

            // Now do your act and asserts
            var response1 = await httpClient.GetAsync("/users");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("/users returned OK with body ");
        }
        
        [Fact]
        public async Task MockMultipleHttpResponses()
        {
            // You can mock multiple responses
            var httpResponseMock1 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/users"))
                .RespondWith((request, response) => response.StatusCode = StatusCodes.Status200OK)
                .Build();
            var httpResponseMock2 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/admin"))
                .RespondWith(async (request, response, cancellationToken) =>
                {
                    response.StatusCode = StatusCodes.Status200OK;
                    await response.WriteAsync("mocked hello", cancellationToken);
                })
                .Build();

            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock1)
                .MockHttpResponse(httpResponseMock2)
                .Build();
            var urls = await httpMockServer.StartAsync();
            // by default the httpMockServer will be listening on a random free http port and a random free https port
            var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
            var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);

            // Now configure the WebApplicationFactory so the HttpClient you intend to test
            // sends requests to the HttpMockServer created
            // The way this is done depends entirely on how you configured your application and thus
            // the code below might not apply for your use case. In the example below, the app will read
            // the base address that the HttpClient will use from the options class named HttpClientsOptions.
            // As such we make sure that its value is set to one of the HttpMockServer's listening URL
            var httpClientsOptions = new HttpClientsOptions
            {
                NamedClientBaseAddress = httpUrl
            };
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IOptions<HttpClientsOptions>>(Options.Create(httpClientsOptions));
                        // inject mocks for any other services
                    });
                })
                .CreateClient();

            // Now do your act and asserts
            var response1 = await httpClient.GetAsync("/users");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("/users returned OK with body ");

            var response2 = await httpClient.GetAsync("/admin");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("/admin returned OK with body mocked hello");
        }

        [Fact]
        public async Task PassInConfigurationValues()
        {
            var httpResponseMock = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/configuration"))
                .RespondWith((request, response) => response.StatusCode = StatusCodes.Status201Created)
                .Build();

            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHostArgs("--urls", "http://*:8822;https://*:9922") // you can pass in any number of configuration values
                //.UseUrl(HttpScheme.Http,8811)  // alternatively to the above call you could do the same
                //.UseUrl(HttpScheme.Https,9911) // using the UseUrl method
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock)                 
                .Build();
            var urls = await httpMockServer.StartAsync();
            // by default the httpMockServer will be listening on a random free http port and a random free https port
            var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
            var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);

            // Now configure the WebApplicationFactory so the HttpClient you intend to test
            // sends requests to the HttpMockServer created
            // The way this is done depends entirely on how you configured your application and thus
            // the code below might not apply for your use case. In the example below, the app will read
            // the base address that the HttpClient will use from the options class named HttpClientsOptions.
            // As such we make sure that its value is set to one of the HttpMockServer's listening URL
            var httpClientsOptions = new HttpClientsOptions
            {
                NamedClientBaseAddress = httpUrl
            };
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IOptions<HttpClientsOptions>>(Options.Create(httpClientsOptions));
                        // inject mocks for any other services
                    });
                })
                .CreateClient();

            // Now do your act and asserts
            var httpResponse = await httpClient.GetAsync("/configuration");
            var rBody = await httpResponse.Content.ReadAsStringAsync();
            rBody.ShouldBe("/configuration returned Created with body ");
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
    }
}
