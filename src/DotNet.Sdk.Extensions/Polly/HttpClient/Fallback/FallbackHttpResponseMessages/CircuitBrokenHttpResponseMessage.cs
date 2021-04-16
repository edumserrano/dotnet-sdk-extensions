using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        public CircuitBrokenHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}