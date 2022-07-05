using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;

/// <summary>
/// Extension methods for the <see cref="ResilienceOptions"/>.
/// </summary>
public static class ResilienceOptionsExtensions
{
    /// <summary>
    /// Adds an instance of <see cref="ResilienceOptions"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Same as doing services.AddOptions for a type ResilienceOptions with a name where services is of type <see cref="IServiceCollection"/>.
    /// This is an alias method to make it easier to add the options type required by the resilience policies.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add the options to.</param>
    /// <param name="name">The name of the options.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for chaining.</returns>
    public static OptionsBuilder<ResilienceOptions> AddHttpClientResilienceOptions(
        this IServiceCollection services,
        string name)
    {
        return services.AddOptions<ResilienceOptions>(name);
    }

    /// <summary>
    /// Retrieves the named <see cref="ResilienceOptions"/> options from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
    /// <param name="name">The name of the options.</param>
    /// <returns>An instance of <see cref="ResilienceOptions"/>.</returns>
    public static ResilienceOptions GetHttpClientResilienceOptions(
        this IServiceProvider serviceProvider,
        string name)
    {
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<ResilienceOptions>>();
        return optionsMonitor.Get(name);
    }
}
