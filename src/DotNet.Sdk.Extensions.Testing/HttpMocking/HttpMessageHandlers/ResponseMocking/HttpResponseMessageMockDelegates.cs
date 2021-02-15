using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Delegate to determine if a <see cref="HttpResponseMessageMock"/> is to be executed or not.
    /// </summary>
    /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> to be executed by the <see cref="HttpResponseMessageMock"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>True if the <see cref="HttpResponseMessageMock"/> should be executed, false otherwise.</returns>
    public delegate Task<bool> HttpResponseMessageMockPredicateDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    /// <summary>
    /// Delegate to specify the <see cref="HttpResponseMessage"/> returned by the <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> executed by the <see cref="HttpResponseMessageMock"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="HttpResponseMessage"/> returned when the <see cref="HttpResponseMessageMock"/> executes.</returns>
    public delegate Task<HttpResponseMessage> HttpResponseMessageMockHandlerDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
}
