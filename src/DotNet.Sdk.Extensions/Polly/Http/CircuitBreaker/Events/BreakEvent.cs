namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;

/// <summary>
/// Contains the event data when the circuit transitions to an <see cref="CircuitState.Open"/> state.
/// </summary>
public sealed class BreakEvent
{
    internal BreakEvent(
        string httpClientName,
        CircuitBreakerOptions circuitBreakerOptions,
        DelegateResult<HttpResponseMessage> lastOutcome,
        CircuitState previousState,
        TimeSpan durationOfBreak,
        Context context)
    {
        HttpClientName = httpClientName;
        CircuitBreakerOptions = circuitBreakerOptions;
        LastOutcome = lastOutcome;
        PreviousState = previousState;
        DurationOfBreak = durationOfBreak;
        Context = context;
    }

    /// <summary>
    /// Gets the name of the HttpClient that triggered this event.
    /// </summary>
    public string HttpClientName { get; }

    /// <summary>
    /// Gets the circuit breaker options applied to the HttpClient that triggered this event.
    /// </summary>
    public CircuitBreakerOptions CircuitBreakerOptions { get; }

    /// <summary>
    /// Gets result from the last HttpClient execution before the circuit state changed to open/isolated.
    /// </summary>
    public DelegateResult<HttpResponseMessage> LastOutcome { get; }

    /// <summary>
    /// Gets the circuit's previous state.
    /// </summary>
    public CircuitState PreviousState { get; }

    /// <summary>
    /// Gets the duration the circuit will stay open before resetting.
    /// </summary>
    public TimeSpan DurationOfBreak { get; }

    /// <summary>
    /// Gets the Polly Context.
    /// </summary>
    public Context Context { get; }
}
