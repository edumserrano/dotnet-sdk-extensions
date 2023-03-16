namespace DotNet.Sdk.Extensions.Testing.HostedServices;

public static partial class RunUntilExtensions
{
    /// <summary>
    /// Terminates the host after the specified timeout.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the startup class used with host.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="WebApplicationFactory{T}"/> that creates the Host to terminate after the timeout.</param>
    /// <param name="timeout">Timeout value.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilTimeoutAsync<T>(this WebApplicationFactory<T> webApplicationFactory, TimeSpan timeout)
        where T : class
    {
        return webApplicationFactory.RunUntilTimeoutAsync(timeout, DefaultScheduler.Instance);
    }

    internal static async Task RunUntilTimeoutAsync<T>(
        this WebApplicationFactory<T> webApplicationFactory,
        TimeSpan timeout,
        IScheduler scheduler)
        where T : class
    {
        if (webApplicationFactory is null)
        {
            throw new ArgumentNullException(nameof(webApplicationFactory));
        }

        static Task<bool> NoOpPredicateAsync() => Task.FromResult(false);
        var options = new RunUntilOptions { Timeout = timeout };
        using var hostRunner = new WebApplicationFactoryHostRunner<T>(webApplicationFactory);
        await hostRunner.RunUntilTimeoutAsync(NoOpPredicateAsync, options, scheduler);
    }

    /// <summary>
    /// Terminates the host after the specified timeout.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to terminate after the timeout.</param>
    /// <param name="timeout">Timeout value.</param>
    /// <returns>The <see cref="Task"/> that will execute the host until it's terminated.</returns>
    public static Task RunUntilTimeoutAsync(this IHost host, TimeSpan timeout)
    {
        return host.RunUntilTimeoutAsync(timeout, DefaultScheduler.Instance);
    }

    internal static async Task RunUntilTimeoutAsync(
        this IHost host,
        TimeSpan timeout,
        IScheduler scheduler)
    {
        if (host is null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        static Task<bool> NoOpPredicateAsync() => Task.FromResult(false);
        var options = new RunUntilOptions { Timeout = timeout };
        using var hostRunner = new DefaultHostRunner(host);
        await hostRunner.RunUntilTimeoutAsync(NoOpPredicateAsync, options, scheduler);
    }

    internal static async Task RunUntilTimeoutAsync(
        this HostRunner hostRunner,
        RunUntilPredicateAsync predicateAsync,
        RunUntilOptions options,
        IScheduler scheduler)
    {
        if (hostRunner is null)
        {
            throw new ArgumentNullException(nameof(hostRunner));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        await hostRunner.StartAsync();
        var hostRunController = new HostRunController(options, scheduler);
        var runUntilResult = await hostRunController.RunUntilAsync(predicateAsync);
        await hostRunner.StopAsync();
        if (runUntilResult != RunUntilResult.TimedOut)
        {
            throw new RunUntilException($"{nameof(RunUntilExtensions)}.{nameof(RunUntilTimeoutAsync)} did NOT time out after {options.Timeout} as expected.");
        }
    }

    internal static async Task RunUntilAsync(
        this HostRunner hostRunner,
        RunUntilPredicateAsync predicateAsync,
        RunUntilOptions options,
        IScheduler scheduler)
    {
        if (hostRunner is null)
        {
            throw new ArgumentNullException(nameof(hostRunner));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        await hostRunner.StartAsync();
        var hostRunController = new HostRunController(options, scheduler);
        var runUntilResult = await hostRunController.RunUntilAsync(predicateAsync);
        await hostRunner.StopAsync();
        if (runUntilResult == RunUntilResult.TimedOut)
        {
            throw new RunUntilException($"{nameof(RunUntilExtensions)}.{nameof(RunUntilAsync)} timed out after {options.Timeout}. This means the Host was shutdown before the {nameof(RunUntilExtensions)}.{nameof(RunUntilAsync)} predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using {nameof(RunUntilExtensions)}.{nameof(RunUntilTimeoutAsync)} instead.");
        }
    }
}
