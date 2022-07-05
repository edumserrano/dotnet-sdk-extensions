namespace DotNet.Sdk.Extensions.Testing.HostedServices;

/// <summary>
/// Options to configure the behavior of the RunUntil host extension.
/// </summary>
public class RunUntilOptions
{
    /// <summary>
    /// Gets or sets period after which the host executing the hosted service will be terminated. Defaults to 5 seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the debugger is attached (eg: debugging tests) the default timeout is set to be very large
    /// (1 day) so that it doesn't affect the debugging experience:
    /// eg: tests failing because the host was stopped due to timeout being reached whilst debugging.
    /// </para>
    /// <para>
    /// You can always set this timeout explicitly and override the default even when debugging.
    /// </para>
    /// </remarks>
    public TimeSpan Timeout { get; set; } = Debugger.IsAttached ? TimeSpan.FromDays(1) : TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets interval of time to check the predicate for the to determine if the host running the hosted service
    /// should be terminated. Defaults to 5 ms.
    /// </summary>
    public TimeSpan PredicateCheckInterval { get; set; } = TimeSpan.FromMilliseconds(5);
}
