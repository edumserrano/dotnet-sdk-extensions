using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.InProcess.ResponseMocking
{
    internal sealed class HttpResponseMessageMockDescriptor
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
            string name,
            HttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            var httpClientName = string.IsNullOrWhiteSpace(name) ? httpClientType.Name : name;
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.TypedClient,
                httpClientName,
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
