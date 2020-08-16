using System.Net.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Defines the properties that represent the outcome of executing a <see cref="IHttpResponseMessageMock"/>.
    /// </summary>
    public interface IHttpResponseMessageMockResult
    {
        /// <summary>
        /// The <see cref="HttpResponseMessage"/> produced by the <see cref="IHttpResponseMessageMock"/>.
        /// </summary>
        HttpResponseMessage HttpResponseMessage { get; }

        /// <summary>
        /// The result of executing the <see cref="IHttpResponseMessageMock"/>.
        /// </summary>
        HttpResponseMessageMockResults Status { get; }
    }
}