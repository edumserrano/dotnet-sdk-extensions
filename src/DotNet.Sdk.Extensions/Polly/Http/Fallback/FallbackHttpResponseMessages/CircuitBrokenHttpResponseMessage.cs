using System;
using System.Net;
using System.Net.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    public enum CircuitBreakerState
    {
        Open,
        Isolated
    }

    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        public CircuitBrokenHttpResponseMessage(CircuitBreakerState circuitBreakerState)
        {
            CircuitBreakerState = circuitBreakerState;
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public CircuitBrokenHttpResponseMessage(CircuitBreakerState circuitBreakerState, Exception exception)
            : this(circuitBreakerState)
        {
            Exception = exception;
        }
        
        public CircuitBreakerState CircuitBreakerState { get; }
        
        public Exception? Exception { get; }
    }
}