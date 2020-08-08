using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders
{
    public enum HttpResponseMockTypes
    {
        TypedClient,
        NamedClient,
        Basic
    }

    public class HttpResponseMockDescriptor
    {
        private HttpResponseMockDescriptor(
            HttpResponseMockTypes httpResponseMockType,
            string httpClientName,
            HttpResponseMockBuilder httpResponseMockBuilder)
        {
            if (httpResponseMockBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpResponseMockBuilder));
            }

            HttpResponseMockType = httpResponseMockType;
            HttpClientName = httpClientName;
            HttpResponseMock = httpResponseMockBuilder.Build();
        }

        public HttpResponseMockTypes HttpResponseMockType { get; }

        public string HttpClientName { get; }

        public HttpResponseMock HttpResponseMock { get; }

        public static HttpResponseMockDescriptor Typed(
            Type httpClientType,
            HttpResponseMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMockDescriptor(
                HttpResponseMockTypes.TypedClient,
                httpClientType.Name,
                httpResponseMockBuilder);
        }

        public static HttpResponseMockDescriptor Named(
            string httpClientName,
            HttpResponseMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMockDescriptor(
                HttpResponseMockTypes.NamedClient,
                httpClientName,
                httpResponseMockBuilder);
        }

        public static HttpResponseMockDescriptor Basic(HttpResponseMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMockDescriptor(
                HttpResponseMockTypes.Basic,
                string.Empty,
                httpResponseMockBuilder);
        }
    }
}