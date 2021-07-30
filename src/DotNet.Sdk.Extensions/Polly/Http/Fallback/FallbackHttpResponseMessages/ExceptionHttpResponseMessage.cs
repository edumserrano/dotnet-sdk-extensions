using System;
using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    /// <summary>
    /// Represents the fallback <see cref="HttpResponseMessage"/> returned
    /// when the HttpClient's request throws a <see cref="HttpRequestException"/>.
    /// </summary>
    public class ExceptionHttpResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// Creates an instance of <see cref="ExceptionHttpResponseMessage"/>
        /// </summary>
        /// <param name="exception">The exception that resulted in the fallback response.</param>
        public ExceptionHttpResponseMessage(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        /// <summary>
        /// Exception that triggered the <see cref="ExceptionHttpResponseMessage"/> fallback response.
        /// </summary>
        public Exception Exception { get; }
    }
}
