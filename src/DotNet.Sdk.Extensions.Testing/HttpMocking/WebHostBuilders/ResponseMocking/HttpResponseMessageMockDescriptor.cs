using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders.ResponseMocking
{
    public enum HttpResponseMessageMockTypes
    {
        TypedClient,
        NamedClient,
        Basic
    }

    public interface IHttpResponseMessageMockDescriptor
    {
        string HttpClientName { get; }

        IHttpResponseMessageMock HttpResponseMock { get; }

        HttpResponseMessageMockTypes HttpResponseMockType { get; }
    }

    internal class HttpResponseMessageMockDescriptor : IHttpResponseMessageMockDescriptor
    {
        private HttpResponseMessageMockDescriptor(
            HttpResponseMessageMockTypes httpResponseMockType,
            string httpClientName,
            IHttpResponseMessageMockBuilder httpResponseMockBuilder)
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

        public IHttpResponseMessageMock HttpResponseMock { get; }

        public static IHttpResponseMessageMockDescriptor Typed(
            Type httpClientType,
            IHttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.TypedClient,
                httpClientType.Name,
                httpResponseMockBuilder);
        }

        public static IHttpResponseMessageMockDescriptor Named(
            string httpClientName,
            IHttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.NamedClient,
                httpClientName,
                httpResponseMockBuilder);
        }

        public static IHttpResponseMessageMockDescriptor Basic(IHttpResponseMessageMockBuilder httpResponseMockBuilder)
        {
            return new HttpResponseMessageMockDescriptor(
                HttpResponseMessageMockTypes.Basic,
                string.Empty,
                httpResponseMockBuilder);
        }
    }
}