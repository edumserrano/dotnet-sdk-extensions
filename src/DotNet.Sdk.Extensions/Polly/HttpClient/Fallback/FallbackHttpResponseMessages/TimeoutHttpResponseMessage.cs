using System.Net;
using System.Net.Http;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class TimeoutHttpResponseMessage : HttpResponseMessage
    {
        public TimeoutHttpResponseMessage(TimeoutRejectedException exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        public TimeoutRejectedException Exception { get; }
    }
}
