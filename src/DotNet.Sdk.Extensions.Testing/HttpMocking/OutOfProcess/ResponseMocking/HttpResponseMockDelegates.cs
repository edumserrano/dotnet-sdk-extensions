using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    /// <summary>
    /// Delegate to determine if a <see cref="HttpResponseMock"/> is to be executed or not.
    /// </summary>
    /// <param name="httpRequest">The <see cref="HttpRequest"/> to be executed by the <see cref="HttpResponseMock"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns></returns>
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(HttpRequest httpRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Delegate to specify the <see cref="HttpResponse"/> returned by the <see cref="HttpResponseMock"/>.
    /// </summary>
    /// <param name="httpRequest">The <see cref="HttpRequest"/> to be executed by the <see cref="HttpResponseMock"/>.</param>
    /// <param name="httpResponse">The <see cref="HttpResponse"/> to return when executing the <see cref="HttpResponseMock"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>Task for the executed operation.</returns>
    public delegate Task HttpResponseMockHandlerAsyncDelegate(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken);
}
