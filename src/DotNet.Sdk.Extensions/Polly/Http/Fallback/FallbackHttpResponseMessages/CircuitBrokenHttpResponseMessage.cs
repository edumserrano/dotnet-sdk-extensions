using System.Net;
using System.Net.Http;
using Polly.CircuitBreaker;

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

        public CircuitBrokenHttpResponseMessage(BrokenCircuitException brokenCircuitException)
            : this(CircuitBreakerState.Open)
        {
            BrokenCircuitException = brokenCircuitException;
        }

        public CircuitBrokenHttpResponseMessage(IsolatedCircuitException isolatedCircuitException)
            : this(CircuitBreakerState.Isolated)
        {
            IsolatedCircuitException = isolatedCircuitException;
        }
        
        public CircuitBreakerState CircuitBreakerState { get; }

        public IsolatedCircuitException? IsolatedCircuitException { get; }

        public BrokenCircuitException? BrokenCircuitException { get; }
    }
}