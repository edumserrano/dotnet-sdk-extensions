namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased.Middlewares;

internal static class ResponseMocksMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseMocks(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.UseMiddleware<ResponseMocksMiddleware>();
    }
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for IMiddleware implementations. Used as generic type param.")]
internal sealed class ResponseMocksMiddleware : IMiddleware
{
    private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

    public ResponseMocksMiddleware(HttpResponseMocksProvider httpResponseMocksProvider)
    {
        _httpResponseMocksProvider = httpResponseMocksProvider ?? throw new ArgumentNullException(nameof(httpResponseMocksProvider));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        foreach (var httpResponseMock in _httpResponseMocksProvider.HttpResponseMocks)
        {
            var result = await httpResponseMock.ExecuteAsync(context);
            if (result == HttpResponseMockResults.Executed)
            {
                return;
            }
        }

        await next(context);
    }
}
