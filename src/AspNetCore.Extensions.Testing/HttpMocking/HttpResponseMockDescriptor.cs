using System;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public class HttpResponseMockDescriptor
    {
        public HttpResponseMockTypes HttpResponseMockType { get; private set; }

        public string HttpClientName { get; private set; }

        public HttpResponseMock HttpResponseMock { get; private set; }

        public static HttpResponseMockDescriptor Typed(
            Type httpClientType,
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (predicateAsync == null) throw new ArgumentNullException(nameof(predicateAsync));
            if (handlerAsync == null) throw new ArgumentNullException(nameof(handlerAsync));

            return new HttpResponseMockDescriptor
            {
                HttpResponseMockType = HttpResponseMockTypes.TypedClient,
                HttpClientName = httpClientType.Name,
                HttpResponseMock = new HttpResponseMock(predicateAsync, handlerAsync)
            };
        }

        public static HttpResponseMockDescriptor Named(
            string httpClientName,
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (string.IsNullOrEmpty(httpClientName))
            {
                throw new ArgumentException($"{nameof(httpClientName)} cannot be null or empty", nameof(httpClientName));
            }
            if (predicateAsync == null) throw new ArgumentNullException(nameof(predicateAsync));
            if (handlerAsync == null) throw new ArgumentNullException(nameof(handlerAsync));

            return new HttpResponseMockDescriptor
            {
                HttpResponseMockType = HttpResponseMockTypes.NamedClient,
                HttpClientName = httpClientName,
                HttpResponseMock = new HttpResponseMock(predicateAsync, handlerAsync)
            };
        }

        public static HttpResponseMockDescriptor Basic(
            HttpResponseMockPredicateAsyncDelegate predicateAsync,
            HttpResponseMockHandlerAsyncDelegate handlerAsync)
        {
            if (predicateAsync == null) throw new ArgumentNullException(nameof(predicateAsync));
            if (handlerAsync == null) throw new ArgumentNullException(nameof(handlerAsync));

            return new HttpResponseMockDescriptor
            {
                HttpResponseMockType = HttpResponseMockTypes.Basic,
                HttpClientName = string.Empty,
                HttpResponseMock = new HttpResponseMock(predicateAsync, handlerAsync)
            };
        }
    }
}