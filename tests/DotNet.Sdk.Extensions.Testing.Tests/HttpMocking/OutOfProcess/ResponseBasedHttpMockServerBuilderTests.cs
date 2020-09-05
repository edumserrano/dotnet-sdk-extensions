using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess
{
    public class ResponseBasedHttpMockServerBuilderTests
    {
        [Fact]
        public async Task MockServerByDefaultProvidesTwoUrls()
        {
            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            var httpResponseMock = httpResponseMockBuilder
                .RespondWith(async (request, response, cancellationToken) =>
                {
                    response.StatusCode = StatusCodes.Status201Created;
                    await response.WriteAsync("hi", cancellationToken);
                })
                .Build();

            await using var mock = new HttpMockServerBuilder()
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock)
                .Build();
            var urls = await mock.StartAsync();
            
            urls.Count.ShouldBe(2);
            urls[0].Scheme.ShouldBe(HttpScheme.Http);
            urls[0].Host.ShouldBe("localhost");
            urls[1].Scheme.ShouldBe(HttpScheme.Https);
            urls[1].Host.ShouldBe("localhost");
        }

        [Fact]
        public async Task Test1()
        {
            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            var httpResponseMock = httpResponseMockBuilder
                .RespondWith(async (request, response, cancellationToken) =>
                {
                    response.StatusCode = StatusCodes.Status201Created;
                    await response.WriteAsync("hi", cancellationToken);
                })
                .Build();

            await using var mock = new HttpMockServerBuilder()
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock)
                .Build();
            var urls = await mock.StartAsync();

            var httpClient = new HttpClient();
            var responseBody = await httpClient.GetStringAsync(urls[0]);
            var responseBody2 = await httpClient.GetStringAsync(urls[1]);
            //var responseBody3 = await httpClient.GetStringAsync(urls[2]);
            responseBody.ShouldBe("hi");
            responseBody2.ShouldBe("hi");
            //responseBody3.ShouldBe("hi");

        }
    }
}
