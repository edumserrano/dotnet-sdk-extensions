using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public delegate Task<bool> HttpResponseMessageMockPredicateDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public delegate Task<HttpResponseMessage> HttpResponseMessageMockHandlerDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public class HttpResponseMessageMock
    {
        private readonly HttpResponseMessageMockPredicateDelegate _predicate;
        private readonly HttpResponseMessageMockHandlerDelegate _handler;

        public HttpResponseMessageMock(
            HttpResponseMessageMockPredicateDelegate predicate,
            HttpResponseMessageMockHandlerDelegate handler)
        {
            _predicate = predicate;
            _handler = handler;
        }

        public async Task<HttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var shouldExecute = await _predicate(request, cancellationToken);
            if (!shouldExecute)
            {
                return HttpResponseMessageMockResult.Skipped();
            }
            var httpResponseMessage = await _handler(request, cancellationToken);
            return HttpResponseMessageMockResult.Executed(httpResponseMessage);
        }
    }
}