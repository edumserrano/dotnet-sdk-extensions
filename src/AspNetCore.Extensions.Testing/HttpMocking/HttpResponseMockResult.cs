using System;
using System.Net.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    public enum HttpResponseMockResults
    {
        Skipped,
        Executed
    }

    public class HttpResponseMockResult
    {
        private HttpResponseMessage? _httpResponseMessage;

        private HttpResponseMockResult() { }

        public HttpResponseMockResults Status { get; private set; }

        public HttpResponseMessage HttpResponseMessage
        {
            get
            {
                if (Status != HttpResponseMockResults.Executed)
                {
                    throw new InvalidOperationException($"Cannot retrieve HttpResponseMessage unless Status is HttpResponseMockResults.Executed. Status is {Status}");
                }

                return _httpResponseMessage!;
            }
            private set => _httpResponseMessage = value;
        }

        public static HttpResponseMockResult Executed(HttpResponseMessage httpResponseMessage)
        {
            return new HttpResponseMockResult
            {
                Status = HttpResponseMockResults.Executed,
                HttpResponseMessage = httpResponseMessage
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