using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;

/// <summary>
/// Provides methods to add a timeout policy to an <see cref="HttpClient"/> via the <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class TimeoutPolicyHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="optionsName">The name of the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        static ITimeoutPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultTimeoutPolicyEventHandler();
        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);
    }

    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder httpClientBuilder,
        Action<TimeoutOptions> configureOptions)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        static ITimeoutPolicyEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultTimeoutPolicyEventHandler();
        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);
    }

    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle timeout events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="optionsName">The name of the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
        where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        static ITimeoutPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);
    }

    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle timeout events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        Action<TimeoutOptions> configureOptions)
        where TPolicyEventHandler : class, ITimeoutPolicyEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        static ITimeoutPolicyEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);
    }

    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="optionsName">The name of the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle timeout events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: eventHandlerFactory);
    }

    /// <summary>
    /// Adds a timeout policy to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the timeout policy to.</param>
    /// <param name="configureOptions">An action to define the <see cref="TimeoutOptions"/> options to use to configure the timeout policy.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle timeout events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder httpClientBuilder,
        Action<TimeoutOptions> configureOptions,
        Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddTimeoutPolicyCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: eventHandlerFactory);
    }

    private static IHttpClientBuilder AddTimeoutPolicyCore(
        this IHttpClientBuilder httpClientBuilder,
        string? optionsName,
        Action<TimeoutOptions>? configureOptions,
        Func<IServiceProvider, ITimeoutPolicyEventHandler> eventHandlerFactory)
    {
        var httpClientName = httpClientBuilder.Name;
        optionsName ??= $"{httpClientName}_timeout_{Guid.NewGuid()}";
        configureOptions ??= _ => { };
        httpClientBuilder.Services
            .AddHttpClientTimeoutOptions(optionsName)
            .ValidateDataAnnotations()
            .Configure(configureOptions);

        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var configuration = eventHandlerFactory(provider);
            var timeoutOptions = provider.GetHttpClientTimeoutOptions(optionsName);
            var timeoutPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(httpClientName, timeoutOptions, configuration);
            return new PolicyHttpMessageHandler(timeoutPolicy);
        });
    }
}
