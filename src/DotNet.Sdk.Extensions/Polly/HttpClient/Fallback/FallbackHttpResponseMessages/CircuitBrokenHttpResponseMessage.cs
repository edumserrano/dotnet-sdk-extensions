using System.Net;
using System.Net.Http;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages
{
    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        public CircuitBrokenHttpResponseMessage()
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public CircuitBrokenHttpResponseMessage(BrokenCircuitException exception) : this()
        {
            Exception = exception;
        }

        public BrokenCircuitException? Exception { get; }
    }
}