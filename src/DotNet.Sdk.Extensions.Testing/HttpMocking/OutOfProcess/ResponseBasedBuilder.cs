namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess;

/// <summary>
/// Provides methods to configure and create an <see cref="HttpMockServer"/> when based on a request/response mocking.
/// </summary>
public class ResponseBasedBuilder
{
    private readonly HttpMockServerArgs _mockServerArgs;
    private readonly List<HttpResponseMock> _httpResponseMocks = [];

    internal ResponseBasedBuilder(HttpMockServerArgs args)
    {
        _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
    }

    /// <summary>
    /// Defines an <see cref="HttpResponseMock"/> to be returned by the server.
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpResponseMock"/>s are executed in the order of which they were added
    /// and only the first whose predicate matches will be executed.
    /// </remarks>
    /// <param name="httpResponseMock">The <see cref="HttpResponseMock"/> to be added to the server's possible response mocks.</param>
    /// <returns>The <see cref="ResponseBasedBuilder"/> for chaining.</returns>
    public ResponseBasedBuilder MockHttpResponse(HttpResponseMock httpResponseMock)
    {
        ArgumentNullException.ThrowIfNull(httpResponseMock);

        _httpResponseMocks.Add(httpResponseMock);
        return this;
    }

    /// <summary>
    /// Defines an <see cref="HttpResponseMock"/> to be returned by the server.
    /// </summary>
    /// <param name="configureHttpResponseMock">An action to configure the <see cref="HttpResponseMock"/> to be added to the server's possible response mocks.</param>
    /// <returns>The <see cref="ResponseBasedBuilder"/> for chaining.</returns>
    public ResponseBasedBuilder MockHttpResponse(Action<HttpResponseMockBuilder> configureHttpResponseMock)
    {
        ArgumentNullException.ThrowIfNull(configureHttpResponseMock);

        var httpResponseMockBuilder = new HttpResponseMockBuilder();
        configureHttpResponseMock(httpResponseMockBuilder);
        var httpResponseMock = httpResponseMockBuilder.Build();
        _httpResponseMocks.Add(httpResponseMock);
        return this;
    }

    /// <summary>
    /// Creates an <see cref="HttpMockServer"/> instance.
    /// </summary>
    /// <returns>The <see cref="HttpMockServer"/> instance.</returns>
    public IHttpMockServer Build()
    {
        return new ResponseBasedHttpMockServer(_mockServerArgs, _httpResponseMocks);
    }
}
