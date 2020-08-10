using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public class HttpResponseMessageMockBuilder
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (httpRequestMessage, cancellationToken) => Task.FromResult(true);
        private HttpResponseMockPredicateAsyncDelegate? _predicateAsync;
        private HttpResponseMockHandlerAsyncDelegate? _handlerAsync;

        public HttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
        {
            // convert to 'async' predicate
            return Where((httpRequestMessage, cancellationToken) => Task.FromResult(predicate(httpRequestMessage)));
        }

        public HttpResponseMessageMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicateAsync)
        {
            if (_predicateAsync != null)
            {
                throw new HttpResponseMessageMockBuilderException("Where condition already configured.");
            }
            _predicateAsync = predicateAsync;
            return this;
        }

        public HttpResponseMessageMockBuilder RespondWith(HttpResponseMessage httpResponseMessage)
        {
            return RespondWith(httpRequestMessage => httpResponseMessage);
        }

        public HttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            // convert to 'async' handler
            return RespondWith((httpRequestMessage, cancellationToken) => Task.FromResult(handler(httpRequestMessage)));
        }

        public HttpResponseMessageMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (_handlerAsync != null)
            {
                throw new HttpResponseMessageMockBuilderException("RespondWith already configured.");
            }
            _handlerAsync = handlerAsync;
            return this;
        }

        public HttpResponseMessageMock Build()
        {
            // predicate is not mandatory. The default predicate represents an always apply condition.
            _predicateAsync ??= _defaultPredicateAsync;
            if (_handlerAsync is null)
            {
                throw new HttpResponseMessageMockBuilderException("HttpResponseMessage not configured for HttpResponseMock. Use RespondWith to configure it.");
            }

            return new HttpResponseMessageMock(_predicateAsync, _handlerAsync);
        }
    }
}
