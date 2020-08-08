using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public class HttpResponseMockBuilder
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (httpRequestMessage, cancellationToken) => Task.FromResult(true);
        private HttpResponseMockPredicateAsyncDelegate? _predicateAsync;
        private HttpResponseMockHandlerAsyncDelegate? _handlerAsync;
        private Type? _httpClientType;
        private string? _httpClientName;
        private HttpClientMockTypes _httpClientMockType = HttpClientMockTypes.Undefined;

        private enum HttpClientMockTypes
        {
            Undefined,
            Typed,
            Named,
            Basic
        }

        public HttpResponseMockBuilder ForTypedClient<TClient>()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientType = typeof(TClient);
            _httpClientMockType = HttpClientMockTypes.Typed;
            return this;
        }

        public HttpResponseMockBuilder ForNamedClient(string name)
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientName = name;
            _httpClientMockType = HttpClientMockTypes.Named;
            return this;
        }

        public HttpResponseMockBuilder ForBasicClient()
        {
            EnsureHttpClientMockTypeIsDefinedOnlyOnce();
            _httpClientMockType = HttpClientMockTypes.Basic;
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
            // predicate is not mandatory. The default predicate represents an always apply condition.
            _predicateAsync ??= _defaultPredicateAsync;
            if (_handlerAsync is null)
            {
                throw new HttpResponseMockBuilderException("HttpResponseMessage not configured for HttpResponseMock. Use RespondWith to configure it.");
            }

            return _httpClientMockType switch
            {
                HttpClientMockTypes.Undefined => throw new HttpResponseMockBuilderException("Client type not configured for HttpResponseMock. Use ForTypedClient, ForNamedClient or ForBasicClient to configure it."),
                HttpClientMockTypes.Typed => HttpResponseMockDescriptor.Typed(_httpClientType!, _predicateAsync, _handlerAsync),
                HttpClientMockTypes.Named => HttpResponseMockDescriptor.Named(_httpClientName!, _predicateAsync, _handlerAsync),
                HttpClientMockTypes.Basic => HttpResponseMockDescriptor.Basic(_predicateAsync, _handlerAsync),
                _ => throw new ArgumentOutOfRangeException(nameof(_httpClientMockType))
            };
        }

        private void EnsureHttpClientMockTypeIsDefinedOnlyOnce()
        {
            if (_httpClientMockType != HttpClientMockTypes.Undefined)
            {
                throw new HttpResponseMockBuilderException("Client type already configured.");
            }
        }
    }
}
