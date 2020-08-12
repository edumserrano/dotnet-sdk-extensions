using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
{
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

        public HttpResponseMessageMockDescriptor Build()
        {
            return _httpClientMockType switch
            {
                HttpClientMockTypes.Undefined => throw new HttpResponseMessageMockDescriptorBuilderException("Client type not configured for HttpResponseMock. Use ForTypedClient, ForNamedClient or ForBasicClient to configure it."),
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
