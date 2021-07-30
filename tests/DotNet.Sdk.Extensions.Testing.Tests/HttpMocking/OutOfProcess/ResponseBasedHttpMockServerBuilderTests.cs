using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess
{
    [Trait("Category", XUnitCategories.HttpMockingOutOfProcess)]
    public class ResponseBasedHttpMockServerBuilderTests
    {
        [Fact]
        public async Task RepliesAsConfigured2()
        {
            var helloHttpResponseMock = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
                .RespondWith(async (_, response, cancellationToken) =>
                    {
                        response.StatusCode = StatusCodes.Status201Created;
                        await response.WriteAsync("hello", cancellationToken);
                    })
                .Build();

            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(helloHttpResponseMock)
                .Build();
            var urls = await httpMockServer.StartAsync();
            var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);

            var httpClient = new HttpClient();
            var helloHttpResponse = await httpClient.GetAsync($"{httpUrl}/hello");
            helloHttpResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            var helloHttpContent = await helloHttpResponse.Content.ReadAsStringAsync();
            helloHttpContent.ShouldBe("hello");
        }

        /// <summary>
        /// Tests that the response based <see cref="HttpMockServer"/> responds to requests as configured.
        /// This also tests the two ways to provide mocks <seealso cref="ResponseBasedBuilder.MockHttpResponse(HttpResponseMock)"/>
        /// and <seealso cref="ResponseBasedBuilder.MockHttpResponse(Action{HttpResponseMockBuilder})"/>
        /// </summary>
        [Fact]
        public async Task RepliesAsConfigured()
        {
            var helloHttpResponseMock = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
                .RespondWith(async (_, response, cancellationToken) =>
                    {
                        response.StatusCode = StatusCodes.Status201Created;
                        await response.WriteAsync("hello", cancellationToken);
                    })
                .Build();

            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(helloHttpResponseMock)
                .MockHttpResponse(mockBuilder =>
                {
                    mockBuilder.RespondWith((_, response) => response.StatusCode = StatusCodes.Status404NotFound);
                })
                .Build();
            var urls = await httpMockServer.StartAsync();
            var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);

            var httpClient = new HttpClient();
            var defaultHttpResponse = await httpClient.GetAsync($"{httpUrl}/default");
            defaultHttpResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            defaultHttpResponse.Content.Headers.ContentLength.ShouldBe(0);

            var helloHttpResponse = await httpClient.GetAsync($"{httpUrl}/hello");
            helloHttpResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            var helloHttpContent = await helloHttpResponse.Content.ReadAsStringAsync();
            helloHttpContent.ShouldBe("hello");

            // TODO for now don't test https because it fails to run the test on linux based ci agent
            // In linux this test fails with error:
            // System.Net.Http.HttpRequestException : The SSL connection could not be established, see inner exception.
            // because dev certificate does not exist
            // Trying to set up the dev certificate with `dotnet dev-certs https --trust` does not work on linux
            // See https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio#ssl-linux

            //var defaultHttpsResponse = await httpClient.GetAsync($"{httpsUrl}/default");
            //defaultHttpsResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            //defaultHttpsResponse.Content.Headers.ContentLength.ShouldBe(0);

            //var helloHttpsResponse = await httpClient.GetAsync($"{httpsUrl}/hello");
            //helloHttpsResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
            //var helloHttpsContent = await helloHttpsResponse.Content.ReadAsStringAsync();
            //helloHttpsContent.ShouldBe("hello");
        }

        /// <summary>
        /// Tests that the response based <see cref="HttpMockServer"/> evaluates <seealso cref="HttpResponseMock"/>s
        /// in the order of which they are added.
        /// This means that if there are two competing predicates the first one wins.
        /// </summary>
        [Fact]
        public async Task OrderMatters()
        {
            var httpResponseMock1 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
                .RespondWith((_, response) => response.StatusCode = StatusCodes.Status401Unauthorized)
                .Build();
            var httpResponseMock2 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
                .RespondWith((_, response) => response.StatusCode = StatusCodes.Status403Forbidden)
                .Build();

            // because we add httpResponseMock1 before httpResponseMock2 and they both
            // have an equal predicate, the one that gets executed is the first one added
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock1)
                .MockHttpResponse(httpResponseMock2)
                .Build();
            var urls = await httpMockServer.StartAsync();
            var httpClient = new HttpClient();
            var helloResponse = await httpClient.GetAsync($"{urls[0]}/hello");
            helloResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            // now create another server where the mocks order is reversed to 
            // show that we indeed get the result from the first registered mock
            await using var mock2 = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock2)
                .MockHttpResponse(httpResponseMock1)
                .Build();
            var urls2 = await mock2.StartAsync();
            var httpClient2 = new HttpClient();
            var helloResponse2 = await httpClient2.GetAsync($"{urls2[0]}/hello");
            helloResponse2.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Tests that the response based <see cref="HttpMockServer"/> returns a default response
        /// if the request does not match any of the provided mock.
        /// </summary>
        [Fact]
        public async Task NoMocksMatch()
        {
            var httpResponseMock1 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/hello", StringComparison.OrdinalIgnoreCase))
                .RespondWith((_, response) => response.StatusCode = StatusCodes.Status200OK)
                .Build();
            var httpResponseMock2 = new HttpResponseMockBuilder()
                .Where(httpRequest => httpRequest.Path.Equals("/bye", StringComparison.OrdinalIgnoreCase))
                .RespondWith((_, response) => response.StatusCode = StatusCodes.Status200OK)
                .Build();

            // because we add httpResponseMock1 before httpResponseMock2 and they both
            // have an equal predicate, the one that gets executed is the first one added
            await using var httpMockServer = new HttpMockServerBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .UseHttpResponseMocks()
                .MockHttpResponse(httpResponseMock1)
                .MockHttpResponse(httpResponseMock2)
                .Build();
            var urls = await httpMockServer.StartAsync();
            var httpClient = new HttpClient();
            var defaultResponse = await httpClient.GetAsync($"{urls[0]}/no-match");
            defaultResponse.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
            var defaultResponseBody = await defaultResponse.Content.ReadAsStringAsync();
            defaultResponseBody.ShouldBe("Request did not match any of the provided mocks.");
        }

        /// <summary>
        /// Tests that you cannot provide null mocks to <seealso cref="ResponseBasedBuilder.MockHttpResponse(HttpResponseMock)"/>
        /// and <seealso cref="ResponseBasedBuilder.MockHttpResponse(Action{HttpResponseMockBuilder})"/>
        /// </summary>
        [Fact]
        public void ValidateMocks()
        {
            var exception1 = Should.Throw<ArgumentNullException>(() =>
            {
                new HttpMockServerBuilder()
                    .UseHttpResponseMocks()
                    .MockHttpResponse((HttpResponseMock)null!);
            });
            exception1.Message.ShouldBe("Value cannot be null. (Parameter 'httpResponseMock')");

            var exception2 = Should.Throw<ArgumentNullException>(() =>
            {
                new HttpMockServerBuilder()
                    .UseHttpResponseMocks()
                    .MockHttpResponse((Action<HttpResponseMockBuilder>)null!);
            });
            exception2.Message.ShouldBe("Value cannot be null. (Parameter 'configureHttpResponseMock')");
        }
    }
}
