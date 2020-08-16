using System;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Defines methods to configure and build an <see cref="IHttpResponseMessageMock"/>
    /// </summary>
    public interface IHttpResponseMessageMockBuilder
    {
        /// <summary>
        /// Builds an instance of <see cref="IHttpResponseMessageMock"/>.
        /// </summary>
        /// <returns>The <see cref="IHttpResponseMessageMock"/> instance.</returns>
        IHttpResponseMessageMock Build();

        /// <summary>
        /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
        /// </summary>
        /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
        /// <returns>The <see cref="IHttpResponseMessageMockBuilder"/> for chaining.</returns>
        IHttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler);

        /// <summary>
        /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/> that the mock returns when executed.</param>
        /// <returns>The <see cref="IHttpResponseMessageMockBuilder"/> for chaining.</returns>
        IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessage httpResponseMessage);

        /// <summary>
        /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
        /// </summary>
        /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
        /// <returns>The <see cref="IHttpResponseMessageMockBuilder"/> for chaining.</returns>
        IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler);

        /// <summary>
        /// Define the condition for the <see cref="IHttpResponseMessageMock"/> to be executed.
        /// </summary>
        /// <param name="predicate">The predicate that determines if the <see cref="IHttpResponseMessageMock"/> is executed or not.</param>
        /// <returns>The <see cref="IHttpResponseMessageMockBuilder"/> for chaining.</returns>
        IHttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate);

        /// <summary>
        /// Define the condition for the <see cref="IHttpResponseMessageMock"/> to be executed.
        /// </summary>
        /// <param name="predicate">The predicate that determines if the <see cref="IHttpResponseMessageMock"/> is executed or not.</param>
        /// <returns>The <see cref="IHttpResponseMessageMockBuilder"/> for chaining.</returns>
        IHttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate);
    }
}