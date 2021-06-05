using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.Auxiliary;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.SimpleStartup;
using DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased.StartupWithControllers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.StartupBased
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
     * The startup classes MySimpleMockStartup and MyMockStartupWithControllers are used as a way to configure
     * what the HttpMockServer will return when receiving the calls from the HttpClients used by the real app.
     *
     */
    public sealed class OutOfProcessStartupBasedDemoTests : IClassFixture<OutOfProcessHttpResponseMockingWebApplicationFactory>, IDisposable
    {
        private readonly OutOfProcessHttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public OutOfProcessStartupBasedDemoTests(OutOfProcessHttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        [Fact]
        public async Task SimpleStartup()
        {
            // First configure the HttpMockServer to use a Startup class
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseStartup<MySimpleMockStartup>()
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
            message1.ShouldBe("/users returned OK with body hello from /users");
        }

        [Fact]
        public async Task StartupWithControllers()
        {
            // First configure the HttpMockServer to use a Startup class
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseStartup<MyMockStartupWithControllers>()
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
            message1.ShouldBe("/users returned OK with body hello from /users");

            var response2 = await httpClient.GetAsync("/admin");
            var message2 = await response2.Content.ReadAsStringAsync();
            message2.ShouldBe("/admin returned OK with body hello from /admin");
        }

        [Fact]
        public async Task PassInConfigurationValues()
        {
            // First configure the HttpMockServer to use a Startup class
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHostArgs("--environment","Staging") // we will show that indeed the HttpMockServer has this configuration value set by returning it when calling the /configuration endpoint on the HttpMockServer
                .UseUrl(HttpScheme.Http,8811)  // you can pass in any number of http/https ports
                .UseUrl(HttpScheme.Https,9911) 
                .UseStartup<MyMockStartupWithControllers>()
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
            var response1 = await httpClient.GetAsync("/configuration");
            var message1 = await response1.Content.ReadAsStringAsync();
            message1.ShouldBe("/configuration returned OK with body hello from /configuration and the mock server environment is Staging");
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
    }
}
