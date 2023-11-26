namespace DotNet.Sdk.Extensions.Testing.HostedServices;

/// <summary>
/// Provides extension methods for the RunUntil method on IHost where the predicate is an async function.
/// </summary>
public static partial class RunUntilExtensions
{
    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to execute.</param>
    /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync(this IHost host, RunUntilPredicateAsync predicateAsync)
    {
        var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
        return host.RunUntilAsync(predicateAsync, configureOptionsAction);

        static void DefaultConfigureOptionsDelegate(RunUntilOptions _)
        {
            // default configure options delegate == do nothing
            // use default values of the RunUntilOptions
        }
    }

    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to execute.</param>
    /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
    /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync(
        this IHost host,
        RunUntilPredicateAsync predicateAsync,
        Action<RunUntilOptions> configureOptions)
    {
        return host.RunUntilAsync(predicateAsync, configureOptions, DefaultScheduler.Instance);
    }

    internal static async Task RunUntilAsync(
        this IHost host,
        RunUntilPredicateAsync predicateAsync,
        Action<RunUntilOptions> configureOptions,
        IScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(scheduler);

        var defaultOptions = new RunUntilOptions();
        configureOptions(defaultOptions);
        using var hostRunner = new DefaultHostRunner(host);
        await hostRunner.RunUntilAsync(predicateAsync, defaultOptions, scheduler);
    }
}
