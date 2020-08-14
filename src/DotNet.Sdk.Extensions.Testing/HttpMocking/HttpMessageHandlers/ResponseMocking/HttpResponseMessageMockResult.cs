using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public enum HttpResponseMessageMockResults
    {
        Skipped,
        Executed
    }

    public interface IHttpResponseMessageMockResult
    {
        HttpResponseMessage HttpResponseMessage { get; }

        HttpResponseMessageMockResults Status { get; }
    }

    internal class HttpResponseMessageMockResult : IHttpResponseMessageMockResult
    {
        private HttpResponseMessage? _httpResponseMessage;

        private HttpResponseMessageMockResult() { }

        public HttpResponseMessageMockResults Status { get; private set; }

        public HttpResponseMessage HttpResponseMessage
        {
            get
            {
                if (Status != HttpResponseMessageMockResults.Executed)
                {
                    throw new InvalidOperationException($"Cannot retrieve {nameof(HttpResponseMessage)} unless Status is {HttpResponseMockResults.Executed}. Status is {Status}");
                }

                return _httpResponseMessage!;
            }
            private set => _httpResponseMessage = value;
        }

        public static IHttpResponseMessageMockResult Executed(HttpResponseMessage httpResponseMessage)
        {
            return new HttpResponseMessageMockResult
            {
                Status = HttpResponseMessageMockResults.Executed,
                HttpResponseMessage = httpResponseMessage
            };
        }

        public static IHttpResponseMessageMockResult Skipped()
        {
            return new HttpResponseMessageMockResult
            {
                Status = HttpResponseMessageMockResults.Skipped
            };
        }
    }
}