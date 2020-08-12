using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AspNetCore.Extensions.Testing.Tests
{
    // todo 
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {

            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            var httpResponseMock = httpResponseMockBuilder
                .RespondWith(async (request, response, cancellationToken) =>
                {
                    response.StatusCode = StatusCodes.Status201Created;
                    await response.WriteAsync("hi", cancellationToken);
                    //return Task.CompletedTask;
                })
                .Build();

            await using var mock = new HttpMockServerBuilder()
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock)
                .MockHttpResponse(builder =>
                {
                    builder
                        .Where((request, cancellationToken) => Task.FromResult(true))
                        .RespondWith((request, response, cancellationToken) =>
                        {
                            response.StatusCode = StatusCodes.Status202Accepted;
                            return Task.CompletedTask;
                        });
                })
                .Build();

            var portNumber = "31245";
            var randomPortNumber = "0";
            var serverPortNumber = string.IsNullOrEmpty(portNumber) ? randomPortNumber : portNumber;
            await mock.Start(new[] { "--urls", $"http://*:{serverPortNumber}" });

            var httpClient = new HttpClient();
            var responseBody = await httpClient.GetStringAsync("http://localhost:31245");
            responseBody.ShouldBe("hi");

        }

        [Fact]
        public async Task Test2()
        {

            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            var httpResponseMock = httpResponseMockBuilder
                .RespondWith(async (request, response, cancellationToken) =>
                {
                    response.StatusCode = StatusCodes.Status201Created;
                    await response.WriteAsync("hi", cancellationToken);
                    //return Task.CompletedTask;
                })
                .Build();

            await using var mock = new HttpMockServerBuilder()
                .UseStartup<MyMockStartup>()
                .Build();

            var portNumber = "31245";
            var randomPortNumber = "0";
            var serverPortNumber = string.IsNullOrEmpty(portNumber) ? randomPortNumber : portNumber;
            await mock.Start(new[] { "--urls", $"http://*:{serverPortNumber}" });

            var httpClient = new HttpClient();
            var responseBody = await httpClient.GetAsync("http://localhost:31245");
            responseBody.StatusCode.ShouldBe(HttpStatusCode.Found);

        }

        public class MyMockStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(
                IApplicationBuilder app,
                IWebHostEnvironment env)
            {
                app.Run(httpContext =>
                {
                    httpContext.Response.StatusCode = StatusCodes.Status302Found;
                    return Task.CompletedTask;
                });
            }
        }
    }
}
