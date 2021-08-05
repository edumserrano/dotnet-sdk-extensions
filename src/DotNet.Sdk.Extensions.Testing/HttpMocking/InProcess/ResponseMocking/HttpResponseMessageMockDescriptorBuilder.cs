using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking
{
    /// <summary>
    /// Provides methods to mock an <see cref="HttpResponseMessage"/> for <see cref="HttpClient"/> calls
    /// when doing tests using <see cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, Action{HttpMessageHandlersReplacer})"/>
    /// or <seealso cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks(IWebHostBuilder, HttpResponseMessageMockDescriptorBuilder[])"/>.
    /// </summary>
    /// <remarks>
    /// This requires that the <see cref="HttpClient"/> used on the app has been resolved via the <see cref="IHttpClientFactory"/>.
    /// </remarks>
    public class HttpResponseMessageMockDescriptorBuilder
    {
        private Type? _httpClientType;
        private string? _httpClientName;
        private HttpClientMockTypes _httpClientMockType;
        private readonly HttpResponseMessageMockBuilder _httpResponseMockBuilder;

        private enum HttpClientMockTypes
        {
            Undefined,
            Typed,
            Named,
            Basic,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMessageMockDescriptorBuilder"/> class.
        /// </summary>
        public HttpResponseMessageMockDescriptorBuilder()
        {
            _httpResponseMockBuilder = new HttpResponseMessageMockBuilder();
            _httpClientMockType = HttpClientMockTypes.Undefined;
        }

        /// <summary>
        /// Indicates that the <see cref="HttpResponseMessage"/> will be mocked for a typed instance of HttpClient.
        /// </summary>
        /// <typeparam name="TClient">The <see cref="Type"/> of the <see cref="HttpClient"/> produced via the <see cref="IHttpClientFactory"/>.</typeparam>
        /// <param name="name">The name of the typed <see cref="HttpClient"/>.</param>
        /// <returns>An instance of <see cref="HttpResponseMessageMockBuilder"/> to customize the <see cref="HttpResponseMessage"/> to mock.</returns>
        public HttpResponseMessageMockBuilder ForTypedClient<TClient>(string name = "")
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientType = typeof(TClient);
            _httpClientName = name;
            _httpClientMockType = HttpClientMockTypes.Typed;
            return _httpResponseMockBuilder;
        }

        /// <summary>
        /// Indicates that the <see cref="HttpResponseMessage"/> will be mocked for a named instance of HttpClient.
        /// </summary>
        /// <param name="name">The name the <see cref="IHttpClientFactory"/> uses to create the HttpClient instance.</param>
        /// <returns>An instance of <see cref="HttpResponseMessageMockBuilder"/> to customize the <see cref="HttpResponseMessage"/> to mock.</returns>
        public HttpResponseMessageMockBuilder ForNamedClient(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Must have a value.", nameof(name));
            }

            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientName = name;
            _httpClientMockType = HttpClientMockTypes.Named;
            return _httpResponseMockBuilder;
        }

        /// <summary>
        /// Indicates that the <see cref="HttpResponseMessage"/> will be mocked for a basic instance of HttpClient.
        /// </summary>
        /// <returns>An instance of <see cref="HttpResponseMessageMockBuilder"/> to customize the <see cref="HttpResponseMessage"/> to mock.</returns>
        public HttpResponseMessageMockBuilder ForBasicClient()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientMockType = HttpClientMockTypes.Basic;
            return _httpResponseMockBuilder;
        }

        internal HttpResponseMessageMockDescriptor Build()
        {
            return _httpClientMockType switch
            {
                HttpClientMockTypes.Undefined => throw new InvalidOperationException($"Client type not configured for {nameof(HttpResponseMock)}. Use {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForTypedClient)}, {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForNamedClient)} or {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForBasicClient)} to configure it."),
                HttpClientMockTypes.Typed => HttpResponseMessageMockDescriptor.Typed(_httpClientType!, _httpClientName!, _httpResponseMockBuilder),
                HttpClientMockTypes.Named => HttpResponseMessageMockDescriptor.Named(_httpClientName!, _httpResponseMockBuilder),
                HttpClientMockTypes.Basic => HttpResponseMessageMockDescriptor.Basic(_httpResponseMockBuilder),
                _ => throw new InvalidOperationException($"Unexpected value for {typeof(HttpClientMockTypes)}: {_httpClientMockType}")
            };
        }

        private void EnsureHttpClientMockTypeIsDefinedOnlyOnce()
        {
            if (_httpClientMockType != HttpClientMockTypes.Undefined)
            {
                throw new InvalidOperationException("Client type already configured.");
            }
        }
    }
}
