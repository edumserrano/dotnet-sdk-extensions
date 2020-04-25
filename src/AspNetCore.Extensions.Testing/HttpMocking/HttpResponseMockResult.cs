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
        public HttpResponseMockResults Status { get; private set; }

        public HttpResponseMessage HttpResponseMessage { get; private set; }

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