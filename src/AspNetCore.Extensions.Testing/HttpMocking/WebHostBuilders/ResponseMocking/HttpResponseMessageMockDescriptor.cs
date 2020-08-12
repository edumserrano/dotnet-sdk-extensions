using System;
using AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace AspNetCore.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking
{
    public enum HttpResponseMessageMockTypes
    {
        TypedClient,
        NamedClient,
        Basic
    }

    public class HttpResponseMessageMockDescriptor
    {
        private HttpResponseMessageMockDescriptor(
            HttpResponseMessageMockTypes httpResponseMockType,
            string httpClientName,
            HttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            if (httpResponseMockBuilder is null)
            {
                throw new ArgumentNullException(nameof(httpResponseMockBuilder));
            }

            HttpResponseMockType = httpResponseMockType;
            HttpClientName = httpClientName;
            HttpResponseMock = httpResponseMockBuilder.Build();
        }

        public HttpResponseMessageMockTypes HttpResponseMockType { get; }

        public string HttpClientName { get; }

        public HttpResponseMessageMock HttpResponseMock { get; }

        public static HttpResponseMessageMockDescriptor Typed(
            Type httpClientType,
            HttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.TypedClient,
                httpClientType.Name,
                httpResponseMockBuilder);
        }

        public static HttpResponseMessageMockDescriptor Named(
            string httpClientName,
            HttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.NamedClient,
                httpClientName,
                httpResponseMockBuilder);
        }

        public static HttpResponseMessageMockDescriptor Basic(HttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.Basic,
                string.Empty,
                httpResponseMockBuilder);
        }
    }
}