namespace DotNet.Sdk.Extensions.Options;

/// <summary>
/// Provides extension methods related with <see cref="OptionsBuilder{T}"/>.
/// </summary>
public static class OptionsBuilderExtensions
{
    /// <summary>
    /// Allows resolving the type of the options added to the <see cref="IServiceCollection"/> instead of having to resolve
    /// one of the Options interfaces: <see cref="IOptions{T}"/>, <see cref="IOptionsSnapshot{T}"/> or <see cref="IOptionsMonitor{T}"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options value to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance containing values to bind to the options type.</param>
    /// <returns>The <see cref="OptionsBuilder{T}"/> for chaining.</returns>
    public static OptionsBuilder<T> AddOptionsValue<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddOptions<T>()
            .Bind(configuration)
            .AddOptionsValue();
    }

    /// <summary>
    /// Allows resolving the type of the options added to the <see cref="IServiceCollection"/> instead of having to resolve
    /// one of the Options interfaces: <see cref="IOptions{T}"/>, <see cref="IOptionsSnapshot{T}"/> or <see cref="IOptionsMonitor{T}"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options value to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance containing values to bind to the options type.</param>
    /// <param name="sectionName">The name of the section from <see cref="IConfiguration"/> where values will be binded from.</param>
    /// <returns>The <see cref="OptionsBuilder{T}"/> for chaining.</returns>
    public static OptionsBuilder<T> AddOptionsValue<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services
            .AddOptions<T>()
            .Bind(configuration.GetSection(sectionName))
            .AddOptionsValue();
    }

    /// <summary>
    /// Allows resolving the type of the options added to the <see cref="IServiceCollection"/> instead of having to resolve
    /// one of the Options interfaces: <see cref="IOptions{T}"/>, <see cref="IOptionsSnapshot{T}"/> or <see cref="IOptionsMonitor{T}"/>.
    /// </summary>
    /// <remarks>
    /// You first need to have configured an options of type T on the <see cref="IServiceCollection"/>.
    /// One way of doing this is by using IServiceCollection.AddOptions followed by the OptionsBuilder.Bind method.
    /// </remarks>
    /// <typeparam name="T">The <see cref="Type"/>  of the options to be configured.</typeparam>
    /// <param name="optionsBuilder">The <see cref="OptionsBuilder{T}"/> to add the options value to.</param>
    /// <returns>The <see cref="OptionsBuilder{T}"/> for chaining.</returns>
    public static OptionsBuilder<T> AddOptionsValue<T>(this OptionsBuilder<T> optionsBuilder)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder.Services.AddOptionsValue<T>();
        return optionsBuilder;
    }

    private static void AddOptionsValue<T>(this IServiceCollection services)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<T>>();
            return options.Value;
        });
    }
}
