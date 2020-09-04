using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public class HttpResponseMessageMockBuilderTests
    {
        /// <summary>
        /// Validates the arguments for the <seealso cref="HttpResponseMessageMockBuilder.Where(Func{HttpRequestMessage, bool})"/> method.
        /// </summary>
        [Fact]
        public void Where1ValidateArguments()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<ArgumentNullException>(() => builder.Where((Func<HttpRequestMessage, bool>)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'predicate')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="HttpResponseMessageMockBuilder.Where(HttpResponseMessageMockPredicateDelegate)"/> method.
        /// </summary>
        [Fact]
        public void Where2ValidateArguments()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<ArgumentNullException>(() => builder.Where((HttpResponseMessageMockPredicateDelegate)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'predicate')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="HttpResponseMessageMockBuilder.RespondWith(HttpResponseMessage)"/> method.
        /// </summary>
        [Fact]
        public void Respond1WithValidateArguments()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith((HttpResponseMessage)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'httpResponseMessage')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="HttpResponseMessageMockBuilder.RespondWith(Func{HttpRequestMessage, HttpResponseMessage})"/> method.
        /// </summary>
        [Fact]
        public void Respond2WithValidateArguments()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith((Func<HttpRequestMessage, HttpResponseMessage>)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'handler')");
        }

        /// <summary>
        /// Validates the arguments for the <seealso cref="HttpResponseMessageMockBuilder.RespondWith(HttpResponseMessageMockHandlerDelegate)"/> method.
        /// </summary>
        [Fact]
        public void Respond3WithValidateArguments()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith((HttpResponseMessageMockHandlerDelegate)null!));
            exception.Message.ShouldBe("Value cannot be null. (Parameter 'handler')");
        }

        /// <summary>
        /// Validates that the predicate can only be set once.
        /// </summary>
        [Fact]
        public void WhereCanOnlyBeDefinedOnce()
        {
            var builder = new HttpResponseMessageMockBuilder();
            builder.Where(message => false);
            var exception = Should.Throw<InvalidOperationException>(() => builder.Where(message => true));
            exception.Message.ShouldBe("HttpResponseMessageMockBuilder.Where condition already configured.");
        }
        
        /// <summary>
        /// Validates that the <seealso cref="HttpResponseMessage"/> to be returned can only be set once.
        /// </summary>
        [Fact]
        public void RespondWithCanOnlyBeDefinedOnce()
        {
            var builder = new HttpResponseMessageMockBuilder();
            builder.RespondWith(new HttpResponseMessage(HttpStatusCode.OK));
            var exception = Should.Throw<InvalidOperationException>(() => builder.RespondWith(new HttpResponseMessage(HttpStatusCode.BadRequest)));
            exception.Message.ShouldBe("HttpResponseMessageMockBuilder.RespondWith already configured.");
        }

        /// <summary>
        /// Validates that you must at least specify the <seealso cref="HttpResponseMessage"/> to be returned.
        /// </summary>
        [Fact]
        public void RespondWithIsMandatory()
        {
            var builder = new HttpResponseMessageMockBuilder();
            var exception = Should.Throw<InvalidOperationException>(() => builder.Build());
            exception.Message.ShouldBe("HttpResponseMessage not configured for HttpResponseMock. Use HttpResponseMessageMockBuilder.RespondWith to configure it.");
        }

        /// <summary>
        /// Tests that the <seealso cref="HttpResponseMessageMockBuilder.Build"/> produces mocks with the
        /// expected predicate and response.
        /// </summary>
        //[Fact]
        //public async Task BuildProducesMocksAsExpected()
        //{
        //    var httpResponseMessageMock1 = new HttpResponseMessageMockBuilder()
        //        .RespondWith(new HttpResponseMessage(HttpStatusCode.Accepted))
        //        .Build();

        //    var httpResponseMessageMock2 = new HttpResponseMessageMockBuilder()
        //        .Where(x => x.Method == HttpMethod.Put)
        //        .RespondWith(new HttpResponseMessage(HttpStatusCode.Redirect))
        //        .Build();

        //    var httpResponseMessageMock3 = new HttpResponseMessageMockBuilder()
        //        .Where(x => x.Method == HttpMethod.Get)
        //        .RespondWith(new HttpResponseMessage(HttpStatusCode.OK))
        //        .Build();

        //    // should return an HttpResponseMessage as mocked on httpResponseMessageMock1 because
        //    // no predicate was specified and that means by default it matches any request
        //    var request1 = new HttpRequestMessage(HttpMethod.Get, "https://bing.com");
        //    var response1 = await httpResponseMessageMock1.ExecuteAsync(request1, CancellationToken.None);
        //    response1.Status.ShouldBe(HttpResponseMessageMockResults.Executed);
        //    response1.HttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        //    // should return an HttpResponseMessage as mocked on httpResponseMessageMock2 because
        //    // the request it matches the specified predicate on httpResponseMessageMock2
        //    var request2 = new HttpRequestMessage(HttpMethod.Put, "https://google.com");
        //    var response2 = await httpResponseMessageMock2.ExecuteAsync(request2, CancellationToken.None);
        //    response2.Status.ShouldBe(HttpResponseMessageMockResults.Executed);
        //    response2.HttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        //    // should NOT return the HttpResponseMessage as mocked on httpResponseMessageMock3 because
        //    // the request does NOT matches the specified predicate on httpResponseMessageMock3
        //    var request3 = new HttpRequestMessage(HttpMethod.Post, "https://microsoft.com");
        //    var response3 = await httpResponseMessageMock3.ExecuteAsync(request3, CancellationToken.None);
        //    response3.Status.ShouldBe(HttpResponseMessageMockResults.Skipped);
        //}
    }
}
