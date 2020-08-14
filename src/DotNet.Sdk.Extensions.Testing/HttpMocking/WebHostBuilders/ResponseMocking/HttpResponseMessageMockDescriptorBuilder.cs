using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking
{
    public interface IHttpResponseMessageMockDescriptorBuilder
    {
        IHttpResponseMessageMockDescriptor Build();

        IHttpResponseMessageMockBuilder ForBasicClient();

        IHttpResponseMessageMockBuilder ForNamedClient(string name);

        IHttpResponseMessageMockBuilder ForTypedClient<TClient>();
    }

    internal class HttpResponseMessageMockDescriptorBuilder : IHttpResponseMessageMockDescriptorBuilder
    {
        private Type? _httpClientType;
        private string? _httpClientName;
        private HttpClientMockTypes _httpClientMockType;
        private readonly IHttpResponseMessageMockBuilder _httpResponseMockBuilder;

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

        public IHttpResponseMessageMockBuilder ForTypedClient<TClient>()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientType = typeof(TClient);
            _httpClientMockType = HttpClientMockTypes.Typed;
            return _httpResponseMockBuilder;
        }

        public IHttpResponseMessageMockBuilder ForNamedClient(string name)
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

        public IHttpResponseMessageMockBuilder ForBasicClient()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientMockType = HttpClientMockTypes.Basic;
            return _httpResponseMockBuilder;
        }

        public IHttpResponseMessageMockDescriptor Build()
        {
            return _httpClientMockType switch
            {
                HttpClientMockTypes.Undefined => throw new HttpResponseMessageMockDescriptorBuilderException($"Client type not configured for {nameof(IHttpResponseMock)}. Use {nameof(IHttpResponseMessageMockDescriptorBuilder)}.{nameof(ForTypedClient)}, {nameof(IHttpResponseMessageMockDescriptorBuilder)}.{nameof(ForNamedClient)} or {nameof(IHttpResponseMessageMockDescriptorBuilder)}.{nameof(ForBasicClient)} to configure it."),
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
