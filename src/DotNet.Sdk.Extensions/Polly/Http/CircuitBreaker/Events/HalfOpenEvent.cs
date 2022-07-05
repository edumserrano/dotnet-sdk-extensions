using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

/// <summary>
/// Contains the event data when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again.
/// </summary>
public class HalfOpenEvent
{
    internal HalfOpenEvent(string httpClientName, CircuitBreakerOptions circuitBreakerOptions)
    {
        HttpClientName = httpClientName;
        CircuitBreakerOptions = circuitBreakerOptions;
    }

    /// <summary>
    /// Gets the name of the HttpClient that triggered this event.
    /// </summary>
    public string HttpClientName { get; }

    /// <summary>
    /// Gets the circuit breaker options applied to the HttpClient that triggered this event.
    /// </summary>
    public CircuitBreakerOptions CircuitBreakerOptions { get; }
}
