using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public enum HttpResponseMockResults
    {
        Skipped,
        Executed
    }

    public class HttpResponseMockResult
    {
        private HttpResponse? _httpResponse;

        private HttpResponseMockResult() { }

        public HttpResponseMockResults Status { get; private set; }

        public HttpResponse HttpResponse
        {
            get
            {
                if (Status != HttpResponseMockResults.Executed)
                {
                    throw new InvalidOperationException($"Cannot retrieve {nameof(HttpResponse)} unless Status is {HttpResponseMockResults.Executed}. Status is {Status}");
                }

                return _httpResponse!;
            }
            private set => _httpResponse = value;
        }

        public static HttpResponseMockResult Executed(HttpResponse httpResponseMessage)
        {
            return new HttpResponseMockResult
            {
                Status = HttpResponseMockResults.Executed,
                HttpResponse = httpResponseMessage
            };
        }

        public static HttpResponseMockResult Skipped()
        {
            return new HttpResponseMockResult
            {
                Status = HttpResponseMockResults.Skipped
            };
        }
    }
}