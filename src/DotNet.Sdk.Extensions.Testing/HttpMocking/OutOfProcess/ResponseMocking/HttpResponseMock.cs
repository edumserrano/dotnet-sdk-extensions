using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    public delegate Task<bool> HttpResponseMockPredicateAsyncDelegate(
        HttpRequest httpRequest,
        CancellationToken cancellationToken);

    public delegate Task HttpResponseMockHandlerAsyncDelegate(
        HttpRequest httpRequest,
        HttpResponse httpResponse,
        CancellationToken cancellationToken);

    public interface IHttpResponseMock
    {
        Task<HttpResponseMockResults> ExecuteAsync(HttpContext httpContext);
    }

    internal class HttpResponseMock : IHttpResponseMock
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _predicateAsync;
        private readonly HttpResponseMockHandlerAsyncDelegate _handlerAsync;

        public HttpResponseMock(
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            _predicateAsync = predicateAsync ?? throw new ArgumentNullException(nameof(predicateAsync));
            _handlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
        }

        public async Task<HttpResponseMockResults> ExecuteAsync(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

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