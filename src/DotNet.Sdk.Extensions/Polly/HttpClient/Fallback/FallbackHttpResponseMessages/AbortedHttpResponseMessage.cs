using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        public AbortedHttpResponseMessage(bool triggeredByTimeoutException)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            TriggeredByTimeoutException = triggeredByTimeoutException;
        }

        public bool TriggeredByTimeoutException { get; }
    }
}