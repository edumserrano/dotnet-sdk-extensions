using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.HttpMessageHandlers
{
    [Trait("Category", XUnitCategories.HttpMockingHttpMessageHandlers)]
    public class TestHttpMessageHandlerTests
    {
        /// <summary>
        /// Validates the arguments for the <seealso cref="TestHttpMessageHandler.MockHttpResponse(HttpResponseMessageMock)"/> method.
        /// </summary>
        [Fact]
        public void ValidateArguments1()
        {
            var handler = new TestHttpMessageHandler();
            var exception = Should.Throw<ArgumentNullException>(() => handler.MockHttpResponse((HttpResponseMessageMock)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'httpResponseMock')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="TestHttpMessageHandler.MockHttpResponse(Action{HttpResponseMessageMockBuilder})"/> method.
        /// </summary>
        [Fact]
        public void ValidateArguments2()
        {
            var handler = new TestHttpMessageHandler();
            var exception = Should.Throw<ArgumentNullException>(() => handler.MockHttpResponse((Action<HttpResponseMessageMockBuilder>)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'configure')");
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> throws an exception if it gets executed
        /// but no mocks were defined.
        /// </summary>
        [Fact]
        public async Task NoMockDefined()
        {
            var handler = new TestHttpMessageHandler();
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var exception = await Should.ThrowAsync<InvalidOperationException>(httpMessageInvoker.SendAsync(request, CancellationToken.None));
            exception.Message.ShouldBe("No response mock defined for GET to https://test.com/.");
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> throws an exception if it gets executed
        /// but no mocks are executed because none match the HttpRequestMessage.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task NoMockMatches()
        {
            var httpMockResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("microsoft"))
                    .RespondWith(httpMockResponseMessage);
            });

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var exception = await Should.ThrowAsync<InvalidOperationException>(httpMessageInvoker.SendAsync(request, CancellationToken.None));
            exception.Message.ShouldBe("No response mock defined for GET to https://test.com/.");
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage.
        /// In this test no predicate is defined which means the default "always true" predicate takes effect
        /// and the mock is always returned. 
        /// Using <seealso cref="TestHttpMessageHandler.MockHttpResponse(HttpResponseMessageMock)"/>
        /// </summary>
        [Fact]
        public async Task DefaultPredicate1()
        {
            var httpMockResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            var builder = new HttpResponseMessageMockBuilder();
            var httpResponseMessageMock = builder
                .RespondWith(httpMockResponseMessage)
                .Build();
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(httpResponseMessageMock);

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var httpResponseMessage = await httpMessageInvoker.SendAsync(request, CancellationToken.None);

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        [Fact]
        public async Task DefaultPredicate11()
        {
            var httpMockResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            var builder = new HttpResponseMessageMockBuilder();
            var httpResponseMessageMock = builder
                .RespondWith(httpMockResponseMessage)
                .Build();
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(httpResponseMessageMock);

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var httpResponseMessage = await httpMessageInvoker.SendAsync(request, CancellationToken.None);

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage.
        /// In this test no predicate is defined which means the default "always true" predicate takes effect
        /// and the mock is always returned.
        /// Using <seealso cref="TestHttpMessageHandler.MockHttpResponse(Action{HttpResponseMessageMockBuilder})"/>
        /// </summary>
        [Fact]
        public async Task DefaultPredicate2()
        {
            var handler = new TestHttpMessageHandler();
            handler.MockHttpResponse(builder => builder.RespondWith(new HttpResponseMessage(HttpStatusCode.Created)));

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var httpResponseMessage = await httpMessageInvoker.SendAsync(request, CancellationToken.None);

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage
        /// for the FIRST match.
        /// </summary>
        [Fact]
        public async Task FirstMatchWins()
        {
            var handler = new TestHttpMessageHandler()
                .MockHttpResponse(builder =>
                {
                    builder
                        .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("test.com"))
                        .RespondWith(new HttpResponseMessage(HttpStatusCode.BadRequest));
                })
                .MockHttpResponse(builder =>
                {
                    builder
                        .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("test.com"))
                        .RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                });

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            var httpResponseMessage = await httpMessageInvoker.SendAsync(request, CancellationToken.None);

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage
        /// for the appropriate predicate match.
        /// </summary>
        [Fact]
        public async Task MultipleMocks()
        {
            var handler = new TestHttpMessageHandler()
                .MockHttpResponse(builder =>
                {
                    builder
                        .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("google.com"))
                        .RespondWith(new HttpResponseMessage(HttpStatusCode.BadRequest));
                })
                .MockHttpResponse(builder =>
                {
                    builder
                        .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("microsoft.com"))
                        .RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                });

            using var httpMessageInvoker = new HttpMessageInvoker(handler);

            using var request1 = new HttpRequestMessage(HttpMethod.Get, "https://google.com");
            var httpResponseMessage1 = await httpMessageInvoker.SendAsync(request1, CancellationToken.None);
            httpResponseMessage1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            using var request2 = new HttpRequestMessage(HttpMethod.Get, "https://microsoft.com");
            var httpResponseMessage2 = await httpMessageInvoker.SendAsync(request2, CancellationToken.None);
            httpResponseMessage2.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Tests that the <seealso cref="TestHttpMessageHandler"/> times out as configured.
        /// The timeout behavior is set to mimic .net's behavior so the exception, inner exception
        /// and exception messages should be equal.
        /// </summary>
        [Fact]
        public async Task TimesOut()
        {
            var handler = new TestHttpMessageHandler()
                .MockHttpResponse(builder => builder.TimesOut(TimeSpan.FromMilliseconds(50)));
            using var httpMessageInvoker = new HttpMessageInvoker(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com");

            // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
            // we are using the try/catch code as a workaround
            // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
            TaskCanceledException? expectedException = null;
            try
            {
                await httpMessageInvoker.SendAsync(request, CancellationToken.None);
            }
            catch (TaskCanceledException exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException.ShouldBeOfType<TaskCanceledException>();
            expectedException.InnerException.ShouldBeOfType<TimeoutException>();
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.05 seconds elapsing.");
            expectedException.InnerException.Message.ShouldBe("A task was canceled.");
        }
    }
}
