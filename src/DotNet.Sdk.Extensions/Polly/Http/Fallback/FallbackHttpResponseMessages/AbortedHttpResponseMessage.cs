using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        public AbortedHttpResponseMessage(TaskCanceledException exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
            // on newer versions .NET still throws TaskCanceledException but the inner exception is of type System.TimeoutException.
            // see https://devblogs.microsoft.com/dotnet/net-5-new-networking-improvements/#better-error-handling
            var innerException = exception?.InnerException;
            TriggeredByTimeoutException = innerException is TimeoutException;
        }
        public TaskCanceledException Exception { get; }

        public bool TriggeredByTimeoutException { get; }
    }
}