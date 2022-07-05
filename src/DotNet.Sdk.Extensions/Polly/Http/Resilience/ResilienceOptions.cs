namespace DotNet.Sdk.Extensions.Polly.Http.Resilience;

/// <summary>
/// Represents the options available to configure the resilience policies.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceOptions"/> class.
    /// </summary>
    public ResilienceOptions()
    {
        CircuitBreaker = new CircuitBreakerOptions();
        Timeout = new TimeoutOptions();
        Retry = new RetryOptions();
        EnableFallbackPolicy = true;
        EnableTimeoutPolicy = true;
        EnableRetryPolicy = true;
        EnableCircuitBreakerPolicy = true;
    }

    /// <summary>
    /// Gets or sets options to configure the circuit breaker policy.
    /// </summary>
    [Required]
    public CircuitBreakerOptions CircuitBreaker { get; set; }

    /// <summary>
    /// Gets or sets options to configure the circuit timeout policy.
    /// </summary>
    [Required]
    public TimeoutOptions Timeout { get; set; }

    /// <summary>
    /// Gets or sets options to configure the circuit retry policy.
    /// </summary>
    [Required]
    public RetryOptions Retry { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the fallback policy added to the <see cref="HttpClient"/>.
    /// </summary>
    public bool EnableFallbackPolicy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the retry policy added to the <see cref="HttpClient"/>.
    /// </summary>
    public bool EnableRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the circuit breaker policy added to the <see cref="HttpClient"/>.
    /// </summary>
    public bool EnableCircuitBreakerPolicy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the timeout policy added to the <see cref="HttpClient"/>.
    /// </summary>
    public bool EnableTimeoutPolicy { get; set; }
}
