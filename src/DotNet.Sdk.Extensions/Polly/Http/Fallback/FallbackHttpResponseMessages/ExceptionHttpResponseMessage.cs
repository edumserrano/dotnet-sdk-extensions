using System.Net;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;

/// <summary>
/// Represents the fallback <see cref="HttpResponseMessage"/> returned
/// when the HttpClient's request throws a <see cref="HttpRequestException"/>.
/// </summary>
public class ExceptionHttpResponseMessage : HttpResponseMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHttpResponseMessage"/> class.
    /// </summary>
    /// <param name="exception">The exception that resulted in the fallback response.</param>
    public ExceptionHttpResponseMessage(Exception exception)
    {
        StatusCode = HttpStatusCode.InternalServerError;
        Exception = exception;
    }

    /// <summary>
    /// Gets exception that triggered the <see cref="ExceptionHttpResponseMessage"/> fallback response.
    /// </summary>
    public Exception Exception { get; }
}
