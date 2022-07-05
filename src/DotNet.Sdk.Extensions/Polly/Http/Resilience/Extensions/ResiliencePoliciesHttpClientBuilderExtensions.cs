using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;

/// <summary>
/// Provides methods to add resilience policies to an <see cref="HttpClient"/> via the <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class ResiliencePoliciesHttpClientBuilderExtensions
{
    private class BlankHttpMessageHandler : DelegatingHandler
    {
    }

    /// <summary>
    /// Adds resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);

        static IResiliencePoliciesEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultResiliencePoliciesEventHandler();
    }

    /// <summary>
    /// Adds resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder httpClientBuilder,
        Action<ResilienceOptions> configureOptions)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);

        static IResiliencePoliciesEventHandler EventHandlerFactory(IServiceProvider _) => new DefaultResiliencePoliciesEventHandler();
    }

    /// <summary>
    /// Adds resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle resilience events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName)
        where TPolicyEventHandler : class, IResiliencePoliciesEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: EventHandlerFactory);

        static IResiliencePoliciesEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
    }

    /// <summary>
    /// Adds resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TPolicyEventHandler">The type that will handle resilience events.</typeparam>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies<TPolicyEventHandler>(
        this IHttpClientBuilder httpClientBuilder,
        Action<ResilienceOptions> configureOptions)
        where TPolicyEventHandler : class, IResiliencePoliciesEventHandler
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        httpClientBuilder.Services.TryAddSingleton<TPolicyEventHandler>();
        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: EventHandlerFactory);

        static IResiliencePoliciesEventHandler EventHandlerFactory(IServiceProvider provider) => provider.GetRequiredService<TPolicyEventHandler>();
    }

    /// <summary>
    /// Adds a resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="optionsName">The name of the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle resilience events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: optionsName,
            configureOptions: null,
            eventHandlerFactory: eventHandlerFactory);
    }

    /// <summary>
    /// Adds a resilience policies to the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClientBuilder">The <see cref="IHttpClientBuilder"/> instance to add the resilience policies to.</param>
    /// <param name="configureOptions">An action to define the <see cref="RetryOptions"/> options to use to configure the resilience policies.</param>
    /// <param name="eventHandlerFactory">Delegate to create an instance that will handle resilience events.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for chaining.</returns>
    public static IHttpClientBuilder AddResiliencePolicies(
        this IHttpClientBuilder httpClientBuilder,
        Action<ResilienceOptions> configureOptions,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        if (httpClientBuilder is null)
        {
            throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        return httpClientBuilder.AddResiliencePoliciesCore(
            optionsName: null,
            configureOptions: configureOptions,
            eventHandlerFactory: eventHandlerFactory);
    }

    private static IHttpClientBuilder AddResiliencePoliciesCore(
        this IHttpClientBuilder httpClientBuilder,
        string? optionsName,
        Action<ResilienceOptions>? configureOptions,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        var httpClientName = httpClientBuilder.Name;
        optionsName ??= $"{httpClientName}_resilience_{Guid.NewGuid()}";
        configureOptions ??= _ => { };
        httpClientBuilder.Services
            .AddSingleton<IValidateOptions<ResilienceOptions>, ResilienceOptionsValidation>()
            .AddHttpClientResilienceOptions(optionsName)
            .ValidateDataAnnotations()
            .Configure(configureOptions);

        // here can NOT reuse the other extension methods to add the policies
        // like for instance RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy
        // because those extension methods add the TPolicyEventHandler and the options
        // to the ServiceCollection plus options validation.
        // This would lead to duplicated registrations and incorrect behavior when
        // validating options (multiple validations and multiple error messages when validations fail).
        return httpClientBuilder
            .AddResilienceFallbackPolicy(optionsName, eventHandlerFactory)
            .AddResilienceRetryPolicy(optionsName, eventHandlerFactory)
            .AddResilienceCircuitBreakerPolicy(optionsName, eventHandlerFactory)
            .AddResilienceTimeoutPolicy(optionsName, eventHandlerFactory);
    }

    private static IHttpClientBuilder AddResilienceFallbackPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
            if (!resilienceOptions.EnableFallbackPolicy)
            {
                return new BlankHttpMessageHandler();
            }

            var policyEventHandler = eventHandlerFactory(provider);
            var retryPolicy = FallbackPolicyFactory.CreateFallbackPolicy(
                httpClientBuilder.Name,
                policyEventHandler);
            return new PolicyHttpMessageHandler(retryPolicy);
        });
    }

    private static IHttpClientBuilder AddResilienceRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
            if (!resilienceOptions.EnableRetryPolicy)
            {
                return new BlankHttpMessageHandler();
            }

            var policyEventHandler = eventHandlerFactory(provider);
            var retryPolicy = RetryPolicyFactory.CreateRetryPolicy(
                httpClientBuilder.Name,
                resilienceOptions.Retry,
                policyEventHandler);
            return new PolicyHttpMessageHandler(retryPolicy);
        });
    }

    private static IHttpClientBuilder AddResilienceCircuitBreakerPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
            if (!resilienceOptions.EnableCircuitBreakerPolicy)
            {
                return new BlankHttpMessageHandler();
            }

            var policyEventHandler = eventHandlerFactory(provider);
            var retryPolicy = CircuitBreakerPolicyFactory.CreateCircuitBreakerPolicy(
                httpClientBuilder.Name,
                resilienceOptions.CircuitBreaker,
                policyEventHandler);
            return new PolicyHttpMessageHandler(retryPolicy);
        });
    }

    private static IHttpClientBuilder AddResilienceTimeoutPolicy(
        this IHttpClientBuilder httpClientBuilder,
        string optionsName,
        Func<IServiceProvider, IResiliencePoliciesEventHandler> eventHandlerFactory)
    {
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var resilienceOptions = provider.GetHttpClientResilienceOptions(optionsName);
            if (!resilienceOptions.EnableTimeoutPolicy)
            {
                return new BlankHttpMessageHandler();
            }

            var policyEventHandler = eventHandlerFactory(provider);
            var retryPolicy = TimeoutPolicyFactory.CreateTimeoutPolicy(
                httpClientBuilder.Name,
                resilienceOptions.Timeout,
                policyEventHandler);
            return new PolicyHttpMessageHandler(retryPolicy);
        });
    }
}
