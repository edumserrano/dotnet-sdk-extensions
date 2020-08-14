using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public interface IHttpResponseMessageMockBuilder
    {
        IHttpResponseMessageMock Build();

        IHttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler);

        IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessage httpResponseMessage);

        IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler);

        IHttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate);

        IHttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate);
    }

    internal class HttpResponseMessageMockBuilder : IHttpResponseMessageMockBuilder
    {
        private readonly HttpResponseMessageMockPredicateDelegate _defaultPredicate = (httpRequestMessage, cancellationToken) => Task.FromResult(true);
        private HttpResponseMessageMockPredicateDelegate? _predicateAsync;
        private HttpResponseMessageMockHandlerDelegate? _handlerAsync;

        public IHttpResponseMessageMockBuilder Where(Func<HttpRequestMessage, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            // convert to 'async' predicate
            return Where((httpRequestMessage, cancellationToken) => Task.FromResult(predicate(httpRequestMessage)));
        }

        public IHttpResponseMessageMockBuilder Where(HttpResponseMessageMockPredicateDelegate predicate)
        {
            if (_predicateAsync != null)
            {
                throw new HttpResponseMessageMockBuilderException($"{nameof(IHttpResponseMessageMockBuilder)}.{nameof(IHttpResponseMessageMockBuilder.Where)} condition already configured.");
            }
            _predicateAsync = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return this;
        }

        public IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessage httpResponseMessage)
        {
            return RespondWith(httpRequestMessage => httpResponseMessage);
        }

        public IHttpResponseMessageMockBuilder RespondWith(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            // convert to 'async' handler
            return RespondWith((httpRequestMessage, cancellationToken) => Task.FromResult(handler(httpRequestMessage)));
        }

        public IHttpResponseMessageMockBuilder RespondWith(HttpResponseMessageMockHandlerDelegate handler)
        {
            if (_handlerAsync != null)
            {
                throw new HttpResponseMessageMockBuilderException($"{nameof(IHttpResponseMessageMockBuilder)}.{nameof(IHttpResponseMessageMockBuilder.RespondWith)} already configured.");
            }
            _handlerAsync = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        public IHttpResponseMessageMock Build()
        {
            // predicate is not mandatory. The default predicate represents an always apply condition.
            _predicateAsync ??= _defaultPredicate;
            if (_handlerAsync is null)
            {
                throw new HttpResponseMessageMockBuilderException($"{nameof(HttpResponseMessage)} not configured for {nameof(IHttpResponseMock)}. Use {nameof(IHttpResponseMessageMockBuilder)}.{nameof(IHttpResponseMessageMockBuilder.RespondWith)} to configure it.");
            }

            return new HttpResponseMessageMock(_predicateAsync, _handlerAsync);
        }
    }
}
