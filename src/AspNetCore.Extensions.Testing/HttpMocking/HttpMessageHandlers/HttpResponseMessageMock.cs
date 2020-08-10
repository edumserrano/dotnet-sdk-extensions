using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public delegate Task<HttpResponseMessage> HttpResponseMockHandlerAsyncDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public class HttpResponseMessageMock
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _predicateAsync;
        private readonly HttpResponseMockHandlerAsyncDelegate _handlerAsync;

        public HttpResponseMessageMock(
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            _predicateAsync = predicateAsync;
            _handlerAsync = handlerAsync;
        }

        public async Task<HttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var shouldExecute = await _predicateAsync(request, cancellationToken);
            if (!shouldExecute)
            {
                return HttpResponseMessageMockResult.Skipped();
            }
            var httpResponseMessage = await _handlerAsync(request, cancellationToken);
            return HttpResponseMessageMockResult.Executed(httpResponseMessage);
        }
    }
}