using System;
using System.Diagnostics.CodeAnalysis;
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
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseStartup<MyMockStartup>()
                .Build();
            var urls = await httpMockServer.StartAsync();

            var httpClient = new HttpClient();
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
        [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for Startup type classes. Used as generic type param.")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
        private class MyMockStartup
        {
#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable IDE0060 // Remove unused parameter
            public static void ConfigureServices(IServiceCollection services)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Review unused parameters
            {
            }

            public void Configure(IApplicationBuilder app)
            {
                app.Use(async (httpContext, next) =>
                {
                    if (!httpContext.Request.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
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
