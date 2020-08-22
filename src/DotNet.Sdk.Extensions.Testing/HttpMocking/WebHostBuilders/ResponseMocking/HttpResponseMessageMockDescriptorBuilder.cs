using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking
{
    /// <summary>
    /// Provides methods to mock <see cref="HttpResponseMessage"/> for <see cref="HttpClient"/> calls
    /// when doing tests using <see cref="HttpMockingWebHostBuilderExtensions.UseHttpMocks"/>
    /// </summary>
    /// <remarks>
    /// This requires that the <see cref="HttpClient"/> 
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
            Basic
        }

        public HttpResponseMessageMockDescriptorBuilder()
        {
            _httpResponseMockBuilder = new HttpResponseMessageMockBuilder();
            _httpClientMockType = HttpClientMockTypes.Undefined;
        }

        public HttpResponseMessageMockBuilder ForTypedClient<TClient>()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientType = typeof(TClient);
            _httpClientMockType = HttpClientMockTypes.Typed;
            return _httpResponseMockBuilder;
        }

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
                HttpClientMockTypes.Undefined => throw new HttpResponseMessageMockDescriptorBuilderException($"Client type not configured for {nameof(HttpResponseMock)}. Use {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForTypedClient)}, {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForNamedClient)} or {nameof(HttpResponseMessageMockDescriptorBuilder)}.{nameof(ForBasicClient)} to configure it."),
                HttpClientMockTypes.Typed => HttpResponseMessageMockDescriptor.Typed(_httpClientType!, _httpResponseMockBuilder),
                HttpClientMockTypes.Named => HttpResponseMessageMockDescriptor.Named(_httpClientName!, _httpResponseMockBuilder),
                HttpClientMockTypes.Basic => HttpResponseMessageMockDescriptor.Basic(_httpResponseMockBuilder),
                _ => throw new ArgumentOutOfRangeException(nameof(_httpClientMockType))
            };
        }

        private void EnsureHttpClientMockTypeIsDefinedOnlyOnce()
        {
            if (_httpClientMockType != HttpClientMockTypes.Undefined)
            {
                throw new HttpResponseMessageMockDescriptorBuilderException("Client type already configured.");
            }
        }
    }
}
