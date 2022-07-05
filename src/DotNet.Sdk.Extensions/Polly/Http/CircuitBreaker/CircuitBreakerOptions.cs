using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;

/// <summary>
/// Represents the options available to configure a circuit breaker policy.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure threshold at which the circuit will break, eg 0.5 represents breaking if 50% or more of actions result in a handled failure.
    /// </summary>
    /// <remarks>
    /// Must be a value between <see cref="double.Epsilon"/> and 1.
    /// </remarks>
    [Range(double.Epsilon, 1)]
    public double FailureThreshold { get; set; }

    /// <summary>
    /// Gets or sets the duration of the time slice over which failure ratios are assessed.
    /// </summary>
    /// <remarks>
    /// Must be a value between <see cref="double.Epsilon"/> and <see cref="double.MaxValue"/>.
    /// You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.
    /// </remarks>
    [Range(double.Epsilon, double.MaxValue)]
    public double SamplingDurationInSecs { get; set; }

    /// <summary>
    /// Gets or sets the minimum throughput: this many actions or more must pass through the circuit in the time slice,
    /// for statistics to be considered significant and the circuit-breaker to come into action.
    /// </summary>
    /// <remarks>
    /// Must be a value between 2 and <see cref="int.MaxValue"/>.
    /// </remarks>
    [Range(2, int.MaxValue)]
    public int MinimumThroughput { get; set; }

    /// <summary>
    /// Gets or sets the duration the circuit will stay open before resetting.
    /// </summary>
    /// <remarks>
    /// Must be a value between <see cref="double.Epsilon"/> and <see cref="double.MaxValue"/>.
    /// You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.
    /// </remarks>
    [Range(double.Epsilon, double.MaxValue)]
    public double DurationOfBreakInSecs { get; set; }
}
