using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public delegate Task<bool> HttpResponseMessageMockPredicateDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public delegate Task<HttpResponseMessage> HttpResponseMessageMockHandlerDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);
}
