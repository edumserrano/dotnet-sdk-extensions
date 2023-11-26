namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;

/// <summary>
/// Provides methods to add a fallback policy to an <see cref="HttpClient"/> via the <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class FallbackPolicyHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a fallback to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the fallback policy to.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder)
    {
        ArgumentNullException.ThrowIfNull(httpClientBuilder);

        return httpClientBuilder.AddFallbackPolicy(EventHandlerFactory);

        static IFallbackPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultFallbackPolicyEventHandler();
    }

    /// <summary>
    /// Adds a fallback policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle fallback events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the fallback policy to.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddFallbackPolicy<TPolicyEventHandler>(this IHttpClientBuilder httpClientBuilder)
        where TPolicyEventHandler : class, IFallbackPolicyEventHandler
    {
        ArgumentNullException.ThrowIfNull(httpClientBuilder);

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        return httpClientBuilder.AddFallbackPolicy(EventHandlerFactory);

        static IFallbackPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
    }

    /// <summary>
    /// Adds a fallback policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the fallback policy to.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle fallback events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddFallbackPolicy(
        this IHttpClientBuilder httpClientBuilder,
        Func<IServiceProvider, IFallbackPolicyEventHandler> eventHandlerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpClientBuilder);

        var httpClientName = httpClientBuilder.Name;
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var policyEventHandler = eventHandlerFactory(provider);
            var fallbackPolicy = FallbackPolicyFactory.CreateFallbackPolicy(httpClientName, policyEventHandler);
            return new PolicyHttpMessageHandler(fallbackPolicy);
        });
    }
}
