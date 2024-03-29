namespace DotNet.Sdk.Extensions.Testing.HostedServices;

/// <summary>
/// Provides extension methods for the RunUntil method on WebApplicationFactory where the predicate is an async function.
/// </summary>
public static partial class RunUntilExtensions
{
    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
    /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync<T>(this WebApplicationFactory<T> webApplicationFactory, RunUntilPredicateAsync predicateAsync)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        var configureOptionsAction = new Action<RunUntilOptions>(DefaultConfigureOptionsDelegate);
        return webApplicationFactory.RunUntilAsync(predicateAsync, configureOptionsAction);

        static void DefaultConfigureOptionsDelegate(RunUntilOptions _)
        {
            // default configure options delegate == do nothing
            // use default values of the RunUntilOptions
        }
    }

    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
    /// <param name="predicateAsync">The async predicate to determine when the host should be terminated.</param>
    /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync<T>(
        this WebApplicationFactory<T> webApplicationFactory,
        RunUntilPredicateAsync predicateAsync,
        Action<RunUntilOptions> configureOptions)
        where T : class
    {
        return webApplicationFactory.RunUntilAsync(predicateAsync, configureOptions, DefaultScheduler.Instance);
    }

    internal static async Task RunUntilAsync<T>(
        this WebApplicationFactory<T> webApplicationFactory,
        RunUntilPredicateAsync predicateAsync,
        Action<RunUntilOptions> configureOptions,
        IScheduler scheduler)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(scheduler);

        var defaultOptions = new RunUntilOptions();
        configureOptions(defaultOptions);
        using var hostRunner = new WebApplicationFactoryHostRunner<T>(webApplicationFactory);
        await hostRunner.RunUntilAsync(predicateAsync, defaultOptions, scheduler);
    }
}
