using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        public AbortedHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}