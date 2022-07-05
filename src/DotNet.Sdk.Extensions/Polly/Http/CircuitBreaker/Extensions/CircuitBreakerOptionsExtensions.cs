namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;

/// <summary>
/// Extension methods for the <see cref="CircuitBreakerOptions"/>.
/// </summary>
public static class CircuitBreakerOptionsExtensions
{
    /// <summary>
    /// Adds an instance of <see cref="CircuitBreakerOptions"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Same as doing services.AddOptions for a type CircuitBreakerOptions with a name where services is of type <see cref="IServiceCollection"/>.
    /// This is an alias method to make it easier to add the options type required by the circuit breaker policy.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add the options to.</param>
    /// <param name="name">The name of the options.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for chaining.</returns>
    public static OptionsBuilder<CircuitBreakerOptions> AddHttpClientCircuitBreakerOptions(
        this IServiceCollection services,
        string name)
    {
        return services.AddOptions<CircuitBreakerOptions>(name);
    }

    /// <summary>
    /// Retrieves the named <see cref="CircuitBreakerOptions"/> options from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
    /// <param name="name">The name of the options.</param>
    /// <returns>An instance of <see cref="CircuitBreakerOptions"/>.</returns>
    public static CircuitBreakerOptions GetHttpClientCircuitBreakerOptions(
        this IServiceProvider serviceProvider,
        string name)
    {
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CircuitBreakerOptions>>();
        return optionsMonitor.Get(name);
    }
}
