using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
{
    public class HttpResponseMockDescriptorBuilder
    {
        private Type? _httpClientType;
        private string? _httpClientName;
        private HttpClientMockTypes _httpClientMockType;
        private HttpResponseMockBuilder _httpResponseMockBuilder;

        private enum HttpClientMockTypes
        {
            Undefined,
            Typed,
            Named,
            Basic
        }

        public HttpResponseMockDescriptorBuilder()
        {
            _httpResponseMockBuilder = new HttpResponseMockBuilder();
            _httpClientMockType = HttpClientMockTypes.Undefined;
        }

        public HttpResponseMockBuilder ForTypedClient<TClient>()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientType = typeof(TClient);
            _httpClientMockType = HttpClientMockTypes.Typed;
            return _httpResponseMockBuilder;
        }

        public HttpResponseMockBuilder ForNamedClient(string name)
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientName = name;
            _httpClientMockType = HttpClientMockTypes.Named;
            return _httpResponseMockBuilder;
        }

        public HttpResponseMockBuilder ForBasicClient()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientMockType = HttpClientMockTypes.Basic;
            return _httpResponseMockBuilder;
        }

        public HttpResponseMockDescriptor Build()
        {
            return _httpClientMockType switch
            {
                HttpClientMockTypes.Undefined => throw new HttpResponseMockDescriptorBuilderException("Client type not configured for HttpResponseMock. Use ForTypedClient, ForNamedClient or ForBasicClient to configure it."),
                HttpClientMockTypes.Typed => HttpResponseMockDescriptor.Typed(_httpClientType!, _httpResponseMockBuilder),
                HttpClientMockTypes.Named => HttpResponseMockDescriptor.Named(_httpClientName!, _httpResponseMockBuilder),
                HttpClientMockTypes.Basic => HttpResponseMockDescriptor.Basic(_httpResponseMockBuilder),
                _ => throw new ArgumentOutOfRangeException(nameof(_httpClientMockType))
            };
        }

        private void EnsureHttpClientMockTypeIsDefinedOnlyOnce()
        {
            if (_httpClientMockType != HttpClientMockTypes.Undefined)
            {
                throw new HttpResponseMockDescriptorBuilderException("Client type already configured.");
            }
        }
    }
}
