using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events
{
    /// <summary>
    /// Contains the event data when the circuit resets to a <see cref="CircuitState.Closed"/> state.
    /// </summary>
    public class ResetEvent
    {
        internal ResetEvent(
            string httpClientName, 
            CircuitBreakerOptions circuitBreakerOptions,
            Context context)
        {
            HttpClientName = httpClientName;
            CircuitBreakerOptions = circuitBreakerOptions;
            Context = context;
        }

        /// <summary>
        /// The name of the HttpClient that triggered this event.
        /// </summary>
        public string HttpClientName { get; }

        /// <summary>
        /// The circuit breaker options applied to the HttpClient that triggered this event.
        /// </summary>
        public CircuitBreakerOptions CircuitBreakerOptions { get; }

        /// <summary>
        /// The Polly Context.
        /// </summary>
        public Context Context { get; }
    }
}