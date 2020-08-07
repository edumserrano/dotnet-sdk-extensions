using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public class HttpResponseMock
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<bool>> _predicateAsync;
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerAsync;

        public HttpResponseMock(
            Func<HttpRequestMessage, CancellationToken, Task<bool>> predicateAsync,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerAsync)
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