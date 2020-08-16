using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(HttpRequest httpRequest, CancellationToken cancellationToken);

    public delegate Task HttpResponseMockHandlerAsyncDelegate(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken);
}
