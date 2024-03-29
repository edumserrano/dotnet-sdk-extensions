namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess;

/// <summary>
/// Provides methods to mock an <see cref="HttpResponseMessage"/>.
/// </summary>
public class HttpMessageHandlersReplacer
{
    private readonly IServiceCollection _services;
    private readonly List<HttpResponseMessageMockDescriptorBuilder> _httpResponseMockBuilders;

    internal HttpMessageHandlersReplacer(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _httpResponseMockBuilders = [];
    }

    /// <summary>
    /// Mocks an <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="configure">An action to configure the <see cref="HttpResponseMessage"/> mock.</param>
    /// <returns>The <see cref="HttpMessageHandlersReplacer"/> for chaining.</returns>
    public HttpMessageHandlersReplacer MockHttpResponse(Action<IServiceProvider, HttpResponseMessageMockDescriptorBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var serviceProvider = _services.BuildServiceProvider();
        var builder = new HttpResponseMessageMockDescriptorBuilder();
        configure(serviceProvider, builder);
        _httpResponseMockBuilders.Add(builder);
        return this;
    }

    /// <summary>
    /// Mocks an <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="configure">An action to configure the <see cref="HttpResponseMessage"/> mock.</param>
    /// <returns>The <see cref="HttpMessageHandlersReplacer"/> for chaining.</returns>
    public HttpMessageHandlersReplacer MockHttpResponse(Action<HttpResponseMessageMockDescriptorBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new HttpResponseMessageMockDescriptorBuilder();
        configure(builder);
        _httpResponseMockBuilders.Add(builder);
        return this;
    }

    /// <summary>
    /// Mocks an <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="httpResponseMessageMockDescriptorBuilder">The configuration for what the <see cref="HttpResponseMessage"/> mock should be.</param>
    /// <returns>The <see cref="HttpMessageHandlersReplacer"/> for chaining.</returns>
    public HttpMessageHandlersReplacer MockHttpResponse(HttpResponseMessageMockDescriptorBuilder httpResponseMessageMockDescriptorBuilder)
    {
        ArgumentNullException.ThrowIfNull(httpResponseMessageMockDescriptorBuilder);

        _httpResponseMockBuilders.Add(httpResponseMessageMockDescriptorBuilder);
        return this;
    }

    internal void ApplyHttpResponseMocks()
    {
        /*
         * There are 3 ways to create clients using IHttpClientFactory (see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
         *
         * Note that:
         * - We can create HttpClients by doing a Basic usage of the IHttpClientFactory, or we can create Named clients or we can create Typed clients.
         * - When overriding the PrimaryHandler for an HttpClient we want to create one handler per HttpClient.
         * - Regardless of how it was created via the IHttpClientFactory, each HttpClient internally will have a name. For typed clients it's the name of the type,
         * for named clients it's the name it was given when adding the HttpClient, for HttpClients created via the Basic usage the name will be an empty string.
         *
         * What we do here is aggregate all the HttpResponseMocks into one TestHttpMessageHandler per HttpClient (by grouping by HttpClientName).
         * Then we overwrite the Primary Handler for each HttpClient in the IServiceCollection with the corresponding TestHttpMessageHandler.
         *
         */

        var testHttpMessageHandlerDescriptors = _httpResponseMockBuilders
            .Select(x => x.Build())
            .GroupBy(x => x.HttpClientName, StringComparer.InvariantCulture)
            .Select(CreateTestHttpMessageHandlers);
        foreach (var testHttpMessageHandlerDescriptor in testHttpMessageHandlerDescriptors)
        {
            /*
             * this is the exact code that IHttpClientBuilder.ConfigurePrimaryHandler does.
             * For more info see https://github.com/dotnet/runtime/blob/823ec6914c2ea5f9e99bc261c6247e4d78fa1cfd/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientBuilderExtensions.cs#L183
             *
             * By doing this we take control of how the PrimaryHandler is build for the HttpClient by the IHttpClientFactory
             *
             */
            _services.Configure<HttpClientFactoryOptions>(testHttpMessageHandlerDescriptor.HttpClientName, options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b =>
                {
                    b.PrimaryHandler = testHttpMessageHandlerDescriptor.HttpMessageHandler;
                });
            });
        }
    }

    private static TestHttpMessageHandlerDescriptor CreateTestHttpMessageHandlers(IGrouping<string, HttpResponseMessageMockDescriptor> httpResponseMockDescriptorsGrouping)
    {
        ArgumentNullException.ThrowIfNull(httpResponseMockDescriptorsGrouping);

        var httpClientName = httpResponseMockDescriptorsGrouping.Key;
        var httpResponseMockDescriptors = httpResponseMockDescriptorsGrouping.ToList();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        foreach (var httpResponseMockDescriptor in httpResponseMockDescriptors)
        {
            testHttpMessageHandler.MockHttpResponse(httpResponseMockDescriptor.HttpResponseMock);
        }

        return new TestHttpMessageHandlerDescriptor(httpClientName, testHttpMessageHandler);
    }
}
