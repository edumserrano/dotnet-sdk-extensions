namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess;

/// <summary>
/// Provides extension methods to mock <see cref="HttpClient"/> calls on a <see cref="IWebHostBuilder"/>.
/// </summary>
public static class HttpMockingWebHostBuilderExtensions
{
    /// <summary>
    /// Allows mocking <see cref="HttpClient"/> calls when the <see cref="HttpClient"/> used was created by the <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to introduce the mocks to.</param>
    /// <param name="configure">An action to configure an <see cref="HttpResponseMessage"/> mock.</param>
    /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
    public static IWebHostBuilder UseHttpMocks(this IWebHostBuilder webHostBuilder, Action<HttpMessageHandlersReplacer> configure)
    {
        if (webHostBuilder is null)
        {
            throw new ArgumentNullException(nameof(webHostBuilder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        webHostBuilder.ConfigureTestServices(services =>
        {
            var httpMessageHandlersReplacer = new HttpMessageHandlersReplacer(services);
            configure(httpMessageHandlersReplacer);
            httpMessageHandlersReplacer.ApplyHttpResponseMocks();
        });
        return webHostBuilder;
    }

    /// <summary>
    /// Allows mocking <see cref="HttpClient"/> calls when the <see cref="HttpClient"/> used was created by the <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to introduce the mocks to.</param>
    /// <param name="httpResponseMessageMockDescriptorBuilders">Set of <see cref="HttpResponseMessage"/> mocks.</param>
    /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
    public static IWebHostBuilder UseHttpMocks(this IWebHostBuilder webHostBuilder, params HttpResponseMessageMockDescriptorBuilder[] httpResponseMessageMockDescriptorBuilders)
    {
        if (webHostBuilder is null)
        {
            throw new ArgumentNullException(nameof(webHostBuilder));
        }

        if (httpResponseMessageMockDescriptorBuilders is null)
        {
            throw new ArgumentNullException(nameof(httpResponseMessageMockDescriptorBuilders));
        }

        webHostBuilder.ConfigureTestServices(services =>
        {
            var httpMessageHandlersReplacer = new HttpMessageHandlersReplacer(services);
            foreach (var httpResponseMessageMockDescriptorBuilder in httpResponseMessageMockDescriptorBuilders)
            {
                httpMessageHandlersReplacer.MockHttpResponse(httpResponseMessageMockDescriptorBuilder);
            }

            httpMessageHandlersReplacer.ApplyHttpResponseMocks();
        });
        return webHostBuilder;
    }
}
