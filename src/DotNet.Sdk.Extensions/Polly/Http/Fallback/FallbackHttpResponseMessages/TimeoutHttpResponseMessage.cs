using System;
using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    public class TimeoutHttpResponseMessage : HttpResponseMessage
    {
        public TimeoutHttpResponseMessage(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
