using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        public CircuitBrokenHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }

    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        public AbortedHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }

    public class TimeoutHttpResponseMessage : HttpResponseMessage
    {
        public TimeoutHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
