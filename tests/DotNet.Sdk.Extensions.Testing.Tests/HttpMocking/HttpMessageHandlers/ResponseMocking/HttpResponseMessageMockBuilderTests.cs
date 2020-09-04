using System;
using System.Net;
using System.Net.Http;
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
    }
}
