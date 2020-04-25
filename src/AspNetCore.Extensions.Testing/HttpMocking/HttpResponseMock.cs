using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public class HttpResponseMock
    {
        private readonly Func<HttpRequestMessage, Task<bool>> _predicateAsync;
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerAsync;
        
        public HttpResponseMock(
            Func<HttpRequestMessage, Task<bool>> predicateAsync,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> handlerAsync)
        {
            _predicateAsync = predicateAsync;
            _handlerAsync = handlerAsync;
        }
        
        public async Task<HttpResponseMockResult> ExecuteAsync(HttpRequestMessage request)
        {
            var shouldExecute = await _predicateAsync(request);
            if (!shouldExecute)
            {
                return HttpResponseMockResult.Skipped();
            }
            var httpResponseMessage = await _handlerAsync(request);
            return HttpResponseMockResult.Executed(httpResponseMessage);
        }
    }
}