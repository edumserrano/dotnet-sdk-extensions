using System;
using System.Net;
using System.Net.Http;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    /// <summary>
    /// Represents the circuit breaker states possible for a <see cref="CircuitBrokenHttpResponseMessage"/>
    /// fallback response.
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        /// The circuit state is open.
        /// </summary>
        Open,

        /// <summary>
        /// The circuit state is isolated.
        /// </summary>
        Isolated
    }

    /// <summary>
    /// Represents the fallback <see cref="HttpResponseMessage"/> returned
    /// when the circuit breaker's state is open or isolated or when the HttpClient's request
    /// throws an <see cref="IsolatedCircuitException"/> or a <see cref="BrokenCircuitException"/>.
    /// </summary>
    public class CircuitBrokenHttpResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// Creates an instance of <see cref="CircuitBrokenHttpResponseMessage"/>
        /// </summary>
        /// <param name="circuitBreakerState">The state of the circuit breaker that resulted in the fallback response.</param>
        public CircuitBrokenHttpResponseMessage(CircuitBreakerState circuitBreakerState)
        {
            CircuitBreakerState = circuitBreakerState;
            StatusCode = HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Creates an instance of <see cref="CircuitBrokenHttpResponseMessage"/>
        /// </summary>
        /// <param name="circuitBreakerState">The state of the circuit breaker when the exception occurred.</param>
        /// <param name="exception">The exception that resulted in the fallback response.</param>
        public CircuitBrokenHttpResponseMessage(CircuitBreakerState circuitBreakerState, Exception exception)
            : this(circuitBreakerState)
        {
            Exception = exception;
        }

        /// <summary>
        /// The state of the circuit breaker when the fallback response was generated.
        /// </summary>
        public CircuitBreakerState CircuitBreakerState { get; }

        /// <summary>
        /// If available, represents the exception that triggered the
        /// <see cref="CircuitBrokenHttpResponseMessage"/> fallback response.
        /// </summary>
        public Exception? Exception { get; }
    }
}
