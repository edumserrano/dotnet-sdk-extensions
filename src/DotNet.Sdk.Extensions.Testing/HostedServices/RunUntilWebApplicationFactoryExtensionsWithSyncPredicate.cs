namespace DotNet.Sdk.Extensions.Testing.HostedServices;

/// <summary>
/// Provides extension methods for the RunUntil method on WebApplicationFactory where the predicate is a sync function.
/// </summary>
public static partial class RunUntilExtensions
{
    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
    /// <param name="predicate">The predicate to determine when the host should be terminated.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync<T>(this WebApplicationFactory<T> webApplicationFactory, RunUntilPredicate predicate)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(predicate);

        Task<bool> PredicateAsync() => Task.FromResult(predicate());
        return webApplicationFactory.RunUntilAsync(PredicateAsync);
    }

    /// <summary>
    /// Executes the host until the predicate or a timeout is met.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> to execute.</param>
    /// <param name="predicate">The predicate to determine when the host should be terminated.</param>
    /// <param name="configureOptions">Action to configure the option values for the host execution.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilAsync<T>(
        this WebApplicationFactory<T> webApplicationFactory,
        RunUntilPredicate predicate,
        Action<RunUntilOptions> configureOptions)
        where T : class
    {
        return webApplicationFactory.RunUntilAsync(predicate, configureOptions, DefaultScheduler.Instance);
    }

    internal static Task RunUntilAsync<T>(
        this WebApplicationFactory<T> webApplicationFactory,
        RunUntilPredicate predicate,
        Action<RunUntilOptions> configureOptions,
        IScheduler scheduler)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(scheduler);

        Task<bool> PredicateAsync() => Task.FromResult(predicate());
        return webApplicationFactory.RunUntilAsync(PredicateAsync, configureOptions, scheduler);
    }
}
