using System.ComponentModel.DataAnnotations;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout;

/// <summary>
/// Represents the options available to configure a timeout policy.
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// Gets or sets timeout value in seconds.
    /// </summary>
    /// <remarks>
    /// Must be a value between <see cref="double.Epsilon"/> and <see cref="double.MaxValue"/>.
    /// You can represent values smaller than 1 second by using a decimal number such as 0.1 which
    /// would mean a timeout of 100 milliseconds.
    /// </remarks>
    [Range(double.Epsilon, double.MaxValue)]
    public double TimeoutInSecs { get; set; }
}
