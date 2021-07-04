using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares
{
    internal static class ResponseMocksMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseMocks(this IApplicationBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ResponseMocksMiddleware>();
        }
    }

    internal class ResponseMocksMiddleware : IMiddleware
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
