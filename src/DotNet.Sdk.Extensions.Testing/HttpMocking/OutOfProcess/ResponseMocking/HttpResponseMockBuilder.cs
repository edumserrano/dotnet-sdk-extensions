namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

/// <summary>
/// Defines methods to configure and build an <see cref="HttpResponseMock"/>.
/// </summary>
public class HttpResponseMockBuilder
{
    private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (_, _) => Task.FromResult(true);
    private HttpResponseMockPredicateAsyncDelegate? _predicateAsync;
    private HttpResponseMockHandlerAsyncDelegate? _handlerAsync;

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMockBuilder"/> for chaining.</returns>
    public HttpResponseMockBuilder Where(Func<HttpRequest, bool> predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        // convert to 'async' predicate
        return Where((httpRequest, _) => Task.FromResult(predicate(httpRequest)));
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMockBuilder"/> for chaining.</returns>
    public HttpResponseMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicate)
    {
        if (_predicateAsync is not null)
        {
            throw new InvalidOperationException($"{nameof(HttpResponseMockBuilder)}.{nameof(Where)} condition already configured.");
        }

        _predicateAsync = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponse"/> produced by the mock.
    /// </summary>
    /// <param name="configureHttpResponse">An action to configure the <see cref="HttpResponse"/> that the mock returns when executed.</param>
    /// <returns>The <see cref="HttpResponseMockBuilder"/> for chaining.</returns>
    public HttpResponseMockBuilder RespondWith(Action<HttpResponse> configureHttpResponse)
    {
        if (configureHttpResponse is null)
        {
            throw new ArgumentNullException(nameof(configureHttpResponse));
        }

        return RespondWith((_, httpResponse) =>
        {
            configureHttpResponse(httpResponse);
        });
    }

    /// <summary>
    /// Configure the <see cref="HttpResponse"/> produced by the mock.
    /// </summary>
    /// <param name="handler">An action to configure the <see cref="HttpResponse"/> that the mock returns when executed.</param>
    /// <returns>The <see cref="HttpResponseMockBuilder"/> for chaining.</returns>
    public HttpResponseMockBuilder RespondWith(Action<HttpRequest, HttpResponse> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        // convert to 'async' handler
        return RespondWith((httpRequest, httpResponse, _) =>
        {
            handler(httpRequest, httpResponse);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Configure the <see cref="HttpResponse"/> produced by the mock.
    /// </summary>
    /// <param name="handlerAsync">Delegate to configure the <see cref="HttpResponse"/> that the mock returns when executed.</param>
    /// <returns>The <see cref="HttpResponseMockBuilder"/> for chaining.</returns>
    public HttpResponseMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync)
    {
        if (_handlerAsync is not null)
        {
            throw new InvalidOperationException($"{nameof(HttpResponseMockBuilder)}.{nameof(RespondWith)} already configured.");
        }

        _handlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
        return this;
    }

    /// <summary>
    /// Creates an instance of <see cref="HttpResponseMock"/>.
    /// </summary>
    /// <returns>The <see cref="HttpResponseMock"/> instance.</returns>
    public HttpResponseMock Build()
    {
        // predicate is not mandatory. The default predicate represents an always apply condition.
        _predicateAsync ??= _defaultPredicateAsync;
        if (_handlerAsync is null)
        {
            throw new InvalidOperationException($"{nameof(HttpResponse)} not configured for {nameof(HttpResponseMock)}. Use {nameof(HttpResponseMockBuilder)}.{nameof(RespondWith)} to configure it.");
        }

        return new HttpResponseMock(_predicateAsync, _handlerAsync);
    }
}
