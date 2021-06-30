using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess
{
    [Trait("Category", XUnitCategories.HttpMockingOutOfProcess)]
    public class StartupBasedHttpMockServerBuilderTests
    {
        /// <summary>
        /// Tests that the startup based <see cref="HttpMockServer"/> responds to requests as configured.
        /// </summary>
        [Fact]
        public async Task RepliesAsConfigured()
        {
            await using var mock = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseStartup<MyMockStartup>()
                .Build();
            var urls = await mock.StartAsync();

            using var httpClient = new HttpClient();
            var helloResponse = await httpClient.GetAsync($"{urls[0]}/hello");
            helloResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            var helloResponseBody = await helloResponse.Content.ReadAsStringAsync();
            helloResponseBody.ShouldBe("hello");

            var defaultResponse = await httpClient.GetAsync($"{urls[0]}/something");
            defaultResponse.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            defaultResponse.Content.Headers.ContentLength.ShouldBe(0);
        }

        /// <summary>
        /// Startup class to assist with the startup based <seealso cref="HttpMockServer"/> tests.
        /// It's a very basic Startup class but you could use whatever asp.net core configuration
        /// you would like such as adding controllers.
        /// </summary>
        public class MyMockStartup
        {
            public static void ConfigureServices(IServiceCollection services)
            {
            }

            public static void Configure(IApplicationBuilder app)
            {
                app.Use(async (httpContext, next) =>
                {
                    if (!httpContext.Request.Path.Equals("/hello"))
                    {
                        await next();
                        return;
                    }

                    httpContext.Response.StatusCode = StatusCodes.Status201Created;
                    await httpContext.Response.WriteAsync("hello");
                });
                app.Run(httpContext =>
                {
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return Task.CompletedTask;
                });
            }
        }
    }
}
