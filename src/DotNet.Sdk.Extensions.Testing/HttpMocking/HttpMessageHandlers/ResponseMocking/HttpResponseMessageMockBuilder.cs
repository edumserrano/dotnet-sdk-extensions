namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

/// <summary>
/// Defines methods to configure and build an <see cref="HttpResponseMessageMock"/>.
/// </summary>
public class HttpResponseMessageMockBuilder
{
    private readonly HttpResponseMessageMockPredicateDelegate _defaultPredicate = (_, _) => Task.FromResult(true);
    private HttpResponseMessageMockPredicateDelegate? _predicateAsync;
    private HttpResponseMessageMockHandlerDelegate? _handlerAsync;

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        // convert to 'async' predicate
        return Where((httpRequestMessage, _) => Task.FromResult(predicate(httpRequestMessage)));
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate)
    {
        if (_predicateAsync is not null)
        {
            throw new InvalidOperationException($"{nameof(HttpResponseMessageMockBuilder)}.{nameof(Where)} condition already configured.");
        }

        _predicateAsync = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(Func<HttpResponseMessage> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        // convert to 'async' handler
        return RespondWith((_, _) => Task.FromResult(handler()));
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        // convert to 'async' handler
        return RespondWith((httpRequestMessage, _) => Task.FromResult(handler(httpRequestMessage)));
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler)
    {
        if (_handlerAsync is not null)
        {
            throw new InvalidOperationException("Response behavior already configured.");
        }

        _handlerAsync = handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Configures the mock to timeout.
    /// </summary>
    /// <remarks>
    /// This assumes that the <see cref="HttpClient.Timeout"/> has been configured to a value lower
    /// than the <paramref name="timeout"/> value passed in.
    /// </remarks>
    /// <param name="timeout">The value for the timeout.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder TimesOut(TimeSpan timeout)
    {
        if (_handlerAsync is not null)
        {
            throw new InvalidOperationException("Response behavior already configured.");
        }

        _handlerAsync = async (_, cancellationToken) =>
        {
            await Task.Delay(timeout, cancellationToken);
            var exMsg = $"The request should have been aborted but it wasn't. Make sure the HttpClient.Timeout value is set to a value lower than {timeout.TotalSeconds} seconds.";
            throw new TimeoutExpectedException(exMsg);
        };
        return this;
    }

    /// <summary>
    /// Builds an instance of <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    /// <returns>The <see cref="HttpResponseMessageMock"/> instance.</returns>
    public HttpResponseMessageMock Build()
    {
        // predicate is not mandatory. The default predicate represents an always apply condition.
        _predicateAsync ??= _defaultPredicate;
        if (_handlerAsync is null)
        {
            throw new InvalidOperationException($"Response behavior not configured for {nameof(HttpResponseMock)}. Use {nameof(HttpResponseMessageMockBuilder)}.{nameof(RespondWith)} or {nameof(HttpResponseMessageMockBuilder)}.{nameof(TimesOut)} to configure it.");
        }

        return new HttpResponseMessageMock(_predicateAsync, _handlerAsync);
    }
}
