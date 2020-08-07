using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public delegate Task<HttpResponseMessage> HttpResponseMockHandlerAsyncDelegate(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken);

    public class HttpResponseMock
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _predicateAsync;
        private readonly HttpResponseMockHandlerAsyncDelegate _handlerAsync;

        public HttpResponseMock(
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            _predicateAsync = predicateAsync;
            _handlerAsync = handlerAsync;
        }

        public async Task<HttpResponseMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var shouldExecute = await _predicateAsync(request, cancellationToken);
            if (!shouldExecute)
            {
                return HttpResponseMockResult.Skipped();
            }
            var httpResponseMessage = await _handlerAsync(request, cancellationToken);
            return HttpResponseMockResult.Executed(httpResponseMessage);
        }
    }
}