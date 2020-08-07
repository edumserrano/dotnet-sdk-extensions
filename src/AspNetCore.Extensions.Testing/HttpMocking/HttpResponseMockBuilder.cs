using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public class HttpResponseMockBuilder
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (httpRequestMessage, cancellationToken) => Task.FromResult(true);
        private HttpResponseMockPredicateAsyncDelegate _predicateAsync;
        private HttpResponseMockHandlerAsyncDelegate _handlerAsync;
        private Type _httpClientType;
        private string _httpClientName;
        private bool _clientTypeConfigured;

        public HttpResponseMockBuilder ForTypedClient<TClient>()
        {
            if (_clientTypeConfigured)
            {
                throw new HttpResponseMockBuilderException("Client type already configured.");
            }
            _httpClientType = typeof(TClient);
            _clientTypeConfigured = true;
            return this;
        }

        public HttpResponseMockBuilder ForNamedClient(string name)
        {
            if (_clientTypeConfigured)
            {
                throw new HttpResponseMockBuilderException("Client type already configured.");
            }
            _httpClientName = name;
            _clientTypeConfigured = true;
            return this;
        }

        public HttpResponseMockBuilder ForBasicClient()
        {
            if (_clientTypeConfigured)
            {
                throw new HttpResponseMockBuilderException("Client type already configured.");
            }
            _clientTypeConfigured = true;
            return this;
        }

        public HttpResponseMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
        {
            // convert to 'async' predicate
            return Where((httpRequestMessage, cancellationToken) => Task.FromResult(predicate(httpRequestMessage)));
        }

        public HttpResponseMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicateAsync)
        {
            if (_predicateAsync != null)
            {
                throw new HttpResponseMockBuilderException("Where condition already configured.");
            }
            _predicateAsync = predicateAsync;
            return this;
        }

        public HttpResponseMockBuilder RespondWith(HttpResponseMessage httpResponseMessage)
        {
            return RespondWith(httpRequestMessage => httpResponseMessage);
        }

        public HttpResponseMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            // convert to 'async' handler
            return RespondWith((httpRequestMessage, cancellationToken) => Task.FromResult(handler(httpRequestMessage)));
        }

        public HttpResponseMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (_handlerAsync != null)
            {
                throw new HttpResponseMockBuilderException("RespondWith already configured.");
            }
            _handlerAsync = handlerAsync;
            return this;
        }

        public HttpResponseMockDescriptor Build()
        {
            if (!_clientTypeConfigured)
            {
                throw new HttpResponseMockBuilderException("Client type not configured for HttpResponseMock. Use ForTypedClient, ForNamedClient or ForBasicClient to configure it.");
            }
            if (_predicateAsync is null) // predicate is not mandatory, a mock can always be applied. The default predicate represents an always apply condition.
            {
                _predicateAsync = _defaultPredicateAsync;
            }
            if (_handlerAsync is null)
            {
                throw new HttpResponseMockBuilderException("Response message not configured for HttpResponseMock. Use RespondWith to configure it.");
            }

            if (_httpClientType != null)
            {
                return HttpResponseMockDescriptor.Typed(_httpClientType, _predicateAsync, _handlerAsync);
            }
            if (!string.IsNullOrEmpty(_httpClientName))
            {
                return HttpResponseMockDescriptor.Named(_httpClientName, _predicateAsync, _handlerAsync);
            }

            return HttpResponseMockDescriptor.Basic(_predicateAsync, _handlerAsync);
        }
    }
}
