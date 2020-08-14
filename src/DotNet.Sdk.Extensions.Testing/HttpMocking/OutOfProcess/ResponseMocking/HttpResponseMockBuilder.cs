using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking
{
    public interface IHttpResponseMockBuilder
    {
        IHttpResponseMock Build();

        IHttpResponseMockBuilder RespondWith(Action<HttpRequest, HttpResponse> handler);

        IHttpResponseMockBuilder RespondWith(Action<HttpResponse> configureHttpResponse);

        IHttpResponseMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync);

        IHttpResponseMockBuilder Where(Func<HttpRequest, bool> predicate);

        IHttpResponseMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicateAsync);
    }

    internal class HttpResponseMockBuilder : IHttpResponseMockBuilder
    {
        private readonly HttpResponseMockPredicateAsyncDelegate _defaultPredicateAsync = (httpRequest, cancellationToken) => Task.FromResult(true);
        private HttpResponseMockPredicateAsyncDelegate? _predicateAsync;
        private HttpResponseMockHandlerAsyncDelegate? _handlerAsync;

        public IHttpResponseMockBuilder Where(Func<HttpRequest, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            // convert to 'async' predicate
            return Where((httpRequest, cancellationToken) => Task.FromResult(predicate(httpRequest)));
        }

        public IHttpResponseMockBuilder Where(HttpResponseMockPredicateAsyncDelegate predicateAsync)
        {
            if (_predicateAsync != null)
            {
                throw new HttpResponseMockBuilderException($"{nameof(IHttpResponseMockBuilder)}.{nameof(Where)} condition already configured.");
            }
            _predicateAsync = predicateAsync ?? throw new ArgumentNullException(nameof(predicateAsync));
            return this;
        }

        public IHttpResponseMockBuilder RespondWith(Action<HttpResponse> configureHttpResponse)
        {
            if (configureHttpResponse == null) throw new ArgumentNullException(nameof(configureHttpResponse));

            return RespondWith((httpRequest, httpResponse) =>
            {
                configureHttpResponse(httpResponse);
            });
        }

        public IHttpResponseMockBuilder RespondWith(Action<HttpRequest, HttpResponse> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            // convert to 'async' handler
            return RespondWith((httpRequest, httpResponse, cancellationToken) =>
            {
                handler(httpRequest, httpResponse);
                return Task.CompletedTask;
            });
        }

        public IHttpResponseMockBuilder RespondWith(HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (_handlerAsync != null)
            {
                throw new HttpResponseMockBuilderException($"{nameof(IHttpResponseMockBuilder)}.{nameof(RespondWith)} already configured.");
            }
            _handlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
            return this;
        }

        public IHttpResponseMock Build()
        {
            // predicate is not mandatory. The default predicate represents an always apply condition.
            _predicateAsync ??= _defaultPredicateAsync;
            if (_handlerAsync is null)
            {
                throw new HttpResponseMockBuilderException($"{nameof(HttpResponse)} not configured for {nameof(IHttpResponseMock)}. Use {nameof(IHttpResponseMockBuilder)}.{nameof(RespondWith)} to configure it.");
            }

            return new HttpResponseMock(_predicateAsync, _handlerAsync);
        }
    }
}
