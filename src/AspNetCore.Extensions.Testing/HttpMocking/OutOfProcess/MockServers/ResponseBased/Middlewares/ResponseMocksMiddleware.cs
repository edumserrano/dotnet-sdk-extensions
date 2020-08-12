using System;
using System.Threading.Tasks;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares
{
    public static class ResponseMocksMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseMocks(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseMocksMiddleware>();
        }
    }

    public class ResponseMocksMiddleware : IMiddleware
    {
        private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

        public ResponseMocksMiddleware(HttpResponseMocksProvider httpResponseMocksProvider)
        {
            _httpResponseMocksProvider = httpResponseMocksProvider ?? throw new ArgumentNullException(nameof(httpResponseMocksProvider));
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            foreach (var httpResponseMock in _httpResponseMocksProvider.HttpResponseMocks)
            {
                var result = await httpResponseMock.ExecuteAsync(httpContext);
                if (result == HttpResponseMockResults.Executed)
                {
                    return;
                }
            }

            await next(httpContext);
        }
    }
}
