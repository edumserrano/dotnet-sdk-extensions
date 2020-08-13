using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    public class HttpResponseMockBuilder
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (httpRequest, cancellationToken) => Task.FromResult(true);
        private HttpResponseMockPredicateAsyncDelegate? _predicateAsync;
        private HttpResponseMockHandlerAsyncDelegate? _handlerAsync;

        public HttpResponseMockBuilder Where(Func<HttpRequest, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            // convert to 'async' predicate
            return Where((httpRequest, cancellationToken) => Task.FromResult(predicate(httpRequest)));
        }

        public HttpResponseMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicateAsync)
        {
            if (_predicateAsync != null)
            {
                throw new HttpResponseMockBuilderException("Where condition already configured.");
            }
            _predicateAsync = predicateAsync ?? throw new ArgumentNullException(nameof(predicateAsync));
            return this;
        }

        public HttpResponseMockBuilder RespondWith(Action<HttpResponse> configureHttpResponse)
        {
            if (configureHttpResponse == null) throw new ArgumentNullException(nameof(configureHttpResponse));

            return RespondWith((httpRequest, httpResponse) =>
            {
                configureHttpResponse(httpResponse);
            });
        }

        public HttpResponseMockBuilder RespondWith(Action<HttpRequest, HttpResponse> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            // convert to 'async' handler
            return RespondWith((httpRequest, httpResponse, cancellationToken) =>
            {
                handler(httpRequest, httpResponse);
                return Task.CompletedTask;
            });
        }

        public HttpResponseMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (_handlerAsync != null)
            {
                throw new HttpResponseMockBuilderException("RespondWith already configured.");
            }
            _handlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
            return this;
        }

        public HttpResponseMock Build()
        {
            // predicate is not mandatory. The default predicate represents an always apply condition.
            _predicateAsync ??= _defaultPredicateAsync;
            if (_handlerAsync is null)
            {
                throw new HttpResponseMockBuilderException("HttpResponse not configured for HttpResponseMock. Use RespondWith to configure it.");
            }

            return new HttpResponseMock(_predicateAsync, _handlerAsync);
        }
    }
}
