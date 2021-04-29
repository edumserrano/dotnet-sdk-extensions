using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        public CircuitBrokenHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}