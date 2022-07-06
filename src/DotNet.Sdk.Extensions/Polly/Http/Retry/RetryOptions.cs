namespace DotNet.Sdk.Extensions.Polly.Http.Retry;

/// <summary>
/// Represents the options available to configure a retry policy.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    /// <remarks>
    /// Must be a value between 0 and <see cref="int.MaxValue"/>.
    /// </remarks>
    [Range(0, int.MaxValue)]
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the median delay to target before the first retry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must be a value between <see cref="double.Epsilon"/> and <see cref="double.MaxValue"/>.
    /// You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.
    /// </para>
    /// <para>
    /// Choose this value,f (= f * 2^0), both to approximate the first delay, and to scale the remainder of the series.
    /// Subsequent retries will (over a large sample size) have a median approximating retries at time f * 2^1, f * 2^2 ... f * 2^t etc for try t.
    /// The actual amount of delay-before-retry for try t may be distributed between 0 and f * (2^(t+1) - 2^(t-1)) for t >= 2;
    /// or between 0 and f * 2^(t+1), for t is 0 or 1.
    /// </para>
    /// </remarks>
    [Range(double.Epsilon, double.MaxValue)]
    public double MedianFirstRetryDelayInSecs { get; set; }
}
