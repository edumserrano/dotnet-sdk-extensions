using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    /// <summary>
    /// Represents the fallback <see cref="HttpResponseMessage"/> returned
    /// when the HttpClient's request throws a <see cref="TimeoutRejectedException"/>
    /// or a <see cref="TaskCanceledException"/> with an inner exception of <see cref="TimeoutException"/>.
    /// </summary>
    public class TimeoutHttpResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="exception">The exception that resulted in the fallback response.</param>
        public TimeoutHttpResponseMessage(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        /// <summary>
        /// Gets exception that triggered the <see cref="TimeoutHttpResponseMessage"/> fallback response.
        /// </summary>
        public Exception Exception { get; }
    }
}
