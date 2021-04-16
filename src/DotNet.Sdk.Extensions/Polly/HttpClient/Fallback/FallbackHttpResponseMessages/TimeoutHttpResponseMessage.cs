using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class TimeoutHttpResponseMessage : HttpResponseMessage
    {
        public TimeoutHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
