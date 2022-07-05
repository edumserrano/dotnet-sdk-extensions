using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares;

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

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for IMiddleware implementations. Used as generic type param.")]
internal class DefaultResponseMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.StatusCode = StatusCodes.Status501NotImplemented;
        return context.Response.WriteAsync("Request did not match any of the provided mocks.");
    }
}
