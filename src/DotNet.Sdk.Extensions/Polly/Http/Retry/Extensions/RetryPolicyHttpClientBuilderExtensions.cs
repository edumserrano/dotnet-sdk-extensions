namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;

/// <summary>
/// Provides methods to add a retry policy to an <see cref="HttpClient"/> via the <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class RetryPolicyHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);

        static IRetryPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultRetryPolicyEventHandler();
    }

    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        Action<RetryOptions> configureOptions)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);

        static IRetryPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultRetryPolicyEventHandler();
    }

    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle retry events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
        where TPolicyEventHandler : class, IRetryPolicyEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);

        static IRetryPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
    }

    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle retry events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        Action<RetryOptions> configureOptions)
        where TPolicyEventHandler : class, IRetryPolicyEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);

        static IRetryPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
    }

    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle retry events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: eventHandlerFactory);
    }

    /// <summary>
    /// Adds a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the retry policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the retry policy.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle retry events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        Action<RetryOptions> configureOptions,
        Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddRetryPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: eventHandlerFactory);
    }

    private static IHttpClientBuilder AddRetryPolicyCore(
        this IHttpClientBuilder httpClientBuilder,
        string? optionsName,
        Action<RetryOptions>? configureOptions,
        Func<IServiceProvider, IRetryPolicyEventHandler> eventHandlerFactory)
    {
        var httpClientName = httpClientBuilder.Name;
        optionsName ??= $"{httpClientName}_retry_{Guid.NewGuid()}";
        configureOptions ??= _ => { };
        httpClientBuilder.Services
            .AddHttpClientRetryOptions(optionsName)
            .ValidateDataAnnotations()
            .Configure(configureOptions);

        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var policyEventHandler = eventHandlerFactory(provider);
            var retryOptions = provider.GetHttpClientRetryOptions(optionsName);
            var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(httpClientName, retryOptions, policyEventHandler);
            return new PolicyHttpMessageHandler(retryPolicy);
        });
    }
}
