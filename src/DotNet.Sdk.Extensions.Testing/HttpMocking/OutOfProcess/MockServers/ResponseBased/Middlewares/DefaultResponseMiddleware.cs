using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares
{
    internal static class DefaultResponseMiddlewareExtensions
    {
        public static IApplicationBuilder RunDefaultResponse(this IApplicationBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<DefaultResponseMiddleware>();
        }
    }

    internal class DefaultResponseMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
            await httpContext.Response.WriteAsync("Request did not match any of the provided mocks.");
        }
    }
}
