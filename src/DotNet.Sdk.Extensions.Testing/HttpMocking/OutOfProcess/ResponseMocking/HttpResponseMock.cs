namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

/// <summary>
/// Represents the mock for an <see cref="HttpResponse"/>.
/// </summary>
public sealed class HttpResponseMock
{
    private readonly HttpResponseMockPredicateAsyncDelegate _predicateAsync;
    private readonly HttpResponseMockHandlerAsyncDelegate _handlerAsync;

    internal HttpResponseMock(
        HttpResponseMockPredicateAsyncDelegate predicateAsync,
        HttpResponseMockHandlerAsyncDelegate handlerAsync)
    {
        _predicateAsync = predicateAsync ?? throw new ArgumentNullException(nameof(predicateAsync));
        _handlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
    }

    internal async Task<HttpResponseMockResults> ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var shouldExecute = await _predicateAsync(httpContext.Request, httpContext.RequestAborted);
        if (!shouldExecute)
        {
            return HttpResponseMockResults.Skipped;
        }

        await _handlerAsync(httpContext.Request, httpContext.Response, httpContext.RequestAborted);
        return HttpResponseMockResults.Executed;
    }
}
