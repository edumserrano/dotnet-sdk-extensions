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
public class InProcessHttpResponseMessageMockBuilder
{
    private readonly HttpResponseMessageMockBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="InProcessHttpResponseMessageMockBuilder"/> class.
    /// </summary>
    public InProcessHttpResponseMessageMockBuilder()
    {
        _builder = new HttpResponseMessageMockBuilder();
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
    {
        _builder.Where(predicate);
        return this;
    }

    /// <summary>
    /// Define the condition for the <see cref="HttpResponseMessageMock"/> to be executed.
    /// </summary>
    /// <param name="predicate">The predicate that determines if the <see cref="HttpResponseMessageMock"/> is executed or not.</param>
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate)
    {
        _builder.Where(predicate);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder RespondWith(Func<HttpResponseMessage> handler)
    {
        _builder.RespondWith(handler);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _builder.RespondWith(handler);
        return this;
    }

    /// <summary>
    /// Configure the <see cref="HttpResponseMessage"/> produced by the mock.
    /// </summary>
    /// <param name="handler">Function to configure the <see cref="HttpResponseMessage"/> produced by the mock.</param>
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler)
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
    /// <returns>The <see cref="InProcessHttpResponseMessageMockBuilder"/> for chaining.</returns>
    public InProcessHttpResponseMessageMockBuilder TimesOut(TimeSpan timeout)
    {
        _builder.TimesOut(timeout);
        return this;
    }

    internal HttpResponseMessageMock Build()
    {
        return _builder.Build();
    }
}
