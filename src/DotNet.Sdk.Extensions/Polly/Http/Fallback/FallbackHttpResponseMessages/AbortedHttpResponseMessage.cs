using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    /// <summary>
    /// Represents the fallback <see cref="HttpResponseMessage"/> returned
    /// when the HttpClient's request throws a <see cref="TaskCanceledException"/>.
    /// </summary>
    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortedHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="exception">The exception that resulted in the fallback response.</param>
        public AbortedHttpResponseMessage(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        /// <summary>
        /// Gets exception that triggered the <see cref="AbortedHttpResponseMessage"/> fallback response.
        /// </summary>
        public Exception Exception { get; }
    }
}
