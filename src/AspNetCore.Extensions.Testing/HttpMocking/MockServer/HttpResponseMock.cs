using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking.MockServer
{
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(
        HttpRequest httpRequest,
        CancellationToken cancellationToken);

    public delegate Task HttpResponseMockHandlerAsyncDelegate(
        HttpRequest httpRequest,
        HttpResponse httpResponse,
        CancellationToken cancellationToken);

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

        public async Task<HttpResponseMockResults> ExecuteAsync(HttpContext httpContext)
        {
            var shouldExecute = await _predicateAsync(httpContext.Request, httpContext.RequestAborted);
            if (!shouldExecute)
            {
                return HttpResponseMockResults.Skipped;
            }
            await _handlerAsync(httpContext.Request, httpContext.Response, httpContext.RequestAborted);
            return HttpResponseMockResults.Executed;
        }
    }
}