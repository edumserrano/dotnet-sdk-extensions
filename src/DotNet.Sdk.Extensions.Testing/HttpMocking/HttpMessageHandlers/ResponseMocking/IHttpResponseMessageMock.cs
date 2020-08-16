using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public delegate Task<bool> HttpResponseMessageMockPredicateDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public delegate Task<HttpResponseMessage> HttpResponseMessageMockHandlerDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Represents a mock of an <see cref="HttpResponseMessage"/>
    /// </summary>
    public interface IHttpResponseMessageMock
    {
        /// <summary>
        /// Executes the <see cref="HttpResponseMessage"/> mock.
        /// </summary>
        /// <param name="request"> The <see cref="HttpRequestMessage"/> passed to the <see cref="HttpResponseMessage"/> mock execution.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> passed to the <see cref="HttpResponseMessage"/> mock execution.</param>
        /// <returns></returns>
        Task<IHttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}