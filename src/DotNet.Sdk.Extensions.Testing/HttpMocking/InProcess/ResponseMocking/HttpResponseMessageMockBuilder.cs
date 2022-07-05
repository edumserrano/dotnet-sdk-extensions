namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking;

/*
 * Before I was reusing the classs DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking.HttpResponseMessageMockBuilder
 * but that class has a public method named Build which I don't want to expose when the user is building HttpResponseMessageMock
 * using the IWebHostBuilder.UseHttpMocks method.
 *
 * This class is just a wrapper on that class but hides the Build method from the public API.
 */

/// <summary>
/// Defines methods to configure and build an <see cref="HttpResponseMessageMock"/> when mocking HTTP responses using the
/// <see cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/> method.
/// </summary>
public class HttpResponseMessageMockBuilder
{
    private readonly HttpMessageHandlers.ResponseMocking.HttpResponseMessageMockBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseMessageMockBuilder"/> class.
    /// </summary>
    public HttpResponseMessageMockBuilder()
    {
        _builder = new HttpMessageHandlers.ResponseMocking.HttpResponseMessageMockBuilder();
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
    {
        _builder.Where(predicate);
        return this;
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate)
    {
        _builder.Where(predicate);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/> that the mock returns when executed.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(HttpResponseMessage httpResponseMessage)
    {
        _builder.RespondWith(httpResponseMessage);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _builder.RespondWith(handler);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="HttpResponseMessageMockBuilder"/> for chaining.</returns>
    public HttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler)
    {
        _builder.RespondWith(handler);
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
        _builder.TimesOut(timeout);
        return this;
    }

    /// <summary>
    /// Builds an instance of <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    /// <returns>The <see cref="HttpResponseMessageMock"/> instance.</returns>
    internal HttpResponseMessageMock Build()
    {
        return _builder.Build();
    }
}
