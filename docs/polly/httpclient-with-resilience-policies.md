# Add a set of resilience policies to an HttpClient

- [Motivation](#motivation)
- [Requirements](#requirements)
- [How to use](#how-to-use)
  - [Basic example](#basic-example)
  - [ResilienceOptions](#resilienceoptions)
    - [For the timeout policy options](#for-the-timeout-policy-options)
    - [For the retry policy options](#for-the-retry-policy-options)
    - [For the circuit breaker policy options](#for-the-circuit-breaker-policy-options)
    - [Policy control options](#policy-control-options)
  - [Binding appsettings values to the resilience policies options](#binding-appsettings-values-to-the-resilience-policies-options)
  - [Handling events from the resilience policies](#handling-events-from-the-resilience-policies)

## Motivation

Every time I use an `HttpClient` I end up repeating the same [Polly](https://github.com/App-vNext/Polly) usage pattern in my projects to a set of resilience polices such as:

- fallback
- retry
- circuit breaker
- timeout

Plus, at times I want to have the values for configuring the policies read from the `appsettings.json` which further increases the code I keep repeating.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

The extension method provided `AddResiliencePolicies` is an extension to the `IHttpClientBuilder` which is what you use when [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0).

This extension combines 4 extension methods into one:

- [Add a timeout policy to an HttpClient](/docs/polly/httpclient-with-timeout-policy.md)
- [Add a retry policy to an HttpClient](/docs/polly/httpclient-with-retry-policy.md)
- [Add a circuit breaker policy to an HttpClient](/docs/polly/httpclient-with-circuit-breaker-policy.md)
- [Add a fallback policy to an HttpClient](/docs/polly/httpclient-with-fallback-policy.md)

From the documentation of the above 4 extension methods it is usefull to read the following:

- The intro of the [`How to use`](/docs/polly/httpclient-with-fallback-policy.md#how-to-use) section of the AddFallbackPolicy extension method: it explains what the fallback policy is configured to handle and which fallback responses are returned. The same applies to the fallback policy added by the  `AddResiliencePolicies` extension.
- The section [`Differentiate different fallback response types`](/docs/polly/httpclient-with-fallback-policy.md#distinguish-different-fallback-response-types) from the AddFallbackPolicy extension method: the same applies to a response returned by an `HttpClient` configured with the `AddResiliencePolicies` extension.

> **Note**
>
> the variable `services` in the examples below is of type `IServiceCollection`. On the default template
> for a Web API you can access it via `builder.services`. Example:
>
> ```csharp
> var builder = WebApplication.CreateBuilder(args);
> builder.Services.AddControllers();
> ```
>

### Basic example

You can add the resilience policies by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddResiliencePolicies(options =>
    {
        options.Timeout.TimeoutInSecs = 1;

        options.Retry.RetryCount = 2;
        options.Retry.MedianFirstRetryDelayInSecs = 1;

        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.DurationOfBreakInSecs = 30;
        options.CircuitBreaker.SamplingDurationInSecs = 10;
        options.CircuitBreaker.FailureThreshold = 0.5;
    });
```

The above example is the simplest way to use the extension method. Note that:

- even though the example shows adding the resilience policies to a named `HttpClient` you can also add it to typed `HttpClient` because the extension method works on the `IHttpClientBuilder`.

- the configuration of the policies' options is done inline but the extension method is also integrated with the all the ecosystem around the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0) in dotnet core, such as the possibility of binding the options values from the `appsettings`. See [Binding appsettings values to the resilience policies options](#binding-appsettings-values-to-the-resilience-policies-options).

- you can provide a class to handle the events produced by the resilience policies. See [Handling events from the resilience policies](#handling-events-from-the-resilience-policies).

### ResilienceOptions

The `ResilienceOptions` provides the following configuration options for the resilience policies.

#### For the timeout policy options

- `TimeoutInSecs`: Timeout value in seconds. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean a timeout of 100 milliseconds.

#### For the retry policy options

- `RetryCount`: maximum number of retries. Must be a value between zero and `int.MaxValue`.
- `MedianFirstRetryDelayInSecs`:  median delay to target before the first retry. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.

#### For the circuit breaker policy options

- `FailureThreshold`: failure threshold at which the circuit will break, eg 0.5 represents breaking if 50% or more of actions result in a handled failure. Must be a value between `double.Epsilon` and 1.
- `SamplingDurationInSecs`:  duration of the timeslice over which failure ratios are assessed. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.
- `MinimumThroughput`: this many actions or more must pass through the circuit in the timeslice for statistics to be considered significant and the circuit-breaker to come into action. Must be a value between 2 and `int.MaxValue`.
- `DurationOfBreakInSecs`: duration the circuit will stay open before resetting. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.

#### Policy control options

- `EnableFallbackPolicy`: whether or not the fallback policy added to the `HttpClient`. Enabled by default.
- `EnableRetryPolicy`: whether or not the retry policy added to the `HttpClient`. Enabled by default.
- `EnableCircuitBreakerPolicy`: whether or not the circuit breaker policy added to the `HttpClient`. Enabled by default.
- `EnableTimeoutPolicy`: whether or not the timeout policy added to the `HttpClient`. Enabled by default.

If you want to [bind the configuration from the appsettings](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#bind-hierarchical-configuration) remember that the name of the key in the appsettings must match the property names of the `ResilienceOptions` for the bind to work.

### Binding appsettings values to the resilience policies options

Imagine that you have an appsettings file with the following:

```json
"MyHttpClient": {
    "EnableFallbackPolicy": true,
    "EnableRetryPolicy": true,
    "EnableCircuitBreakerPolicy": true,
    "EnableTimeoutPolicy": true,

    "TimeoutInSecs": "1",

    "RetryCount": "2",
    "MedianFirstRetryDelayInSecs": "1",

    "FailureThreshold": 0.5,
    "SamplingDurationInSecs": 10,
    "MinimumThroughput": 8,
    "DurationOfBreakInSecs": 30
}
```

You can add the set resilience policies that are configured from the values on the appsettings file by doing the following:

```csharp
services
    .AddHttpClientResilienceOptions("my-resilience-options")
    .Bind(Configuration.GetSection("MyHttpClient"));

services
    .AddHttpClient("my-http-client")
    .AddResiliencePolicies("my-resilience-options");
```

The `services.AddHttpClientResilienceOptions` adds a named options of type `ResilienceOptions` and returns an instance of `OptionsBuilder<ResilienceOptions>`, which means you can now use any of the methods provided by dotnet to configure it such as for example:

- [Options validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-validation)
- [Options post-configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-post-configuration)

### Handling events from the resilience policies

This extension method also enables you to access the events provided by all of the resilience Polly policies added to the `HttpClient`.

You can specify a class to handle the events from the policies by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddResiliencePolicies<MyResilienceEventHandler>(options =>
    {
        options.Timeout.TimeoutInSecs = 1;
        options.Retry.RetryCount = 2;
        options.Retry.MedianFirstRetryDelayInSecs = 1;
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.DurationOfBreakInSecs = 30;
        options.CircuitBreaker.SamplingDurationInSecs = 10;
        options.CircuitBreaker.FailureThreshold = 0.5;
    });
```

The `MyResilienceEventHandler` must implement the `IResiliencePoliciesEventHandler` interface.

```csharp
public class MyResilienceEventHandler : IResiliencePoliciesEventHandler
{
    private readonly ILogger<MyResilienceEventHandler> _logger;

    public MyResilienceEventHandler(ILogger<MyResilienceEventHandler> logger)
    {
        _logger = logger;
    }

    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        //do something like logging
        _logger.LogInformation($"A timeout has occurred on the HttpClient {timeoutEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        //do something like logging
        _logger.LogInformation($"Retry {retryEvent.RetryNumber} out of {retryEvent.RetryOptions.RetryCount} for HttpClient {retryEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnBreakAsync(BreakEvent breakEvent)
    {
        //do something like logging
        _logger.LogInformation($"Circuit state transitioned from {breakEvent.PreviousState} to open/isolated for the HttpClient {breakEvent.HttpClientName}. Break will last for {breakEvent.DurationOfBreak}");
        return Task.CompletedTask;
    }

    public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
    {
        //do something like logging
        _logger.LogInformation($"Circuit state transitioned to half open for the HttpClient {halfOpenEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnResetAsync(ResetEvent resetEvent)
    {
        //do something like logging
        _logger.LogInformation($"Circuit state transitioned to closed for the HttpClient {resetEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent)
    {
        //do something like logging
        _logger.LogInformation($"Fallback response returned due to HttpRequestException for the HttpClient {fallbackEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent)
    {
        //do something like logging
        _logger.LogInformation($"Fallback response returned due to timeout for the HttpClient {fallbackEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent)
    {
        //do something like logging
        _logger.LogInformation($"Fallback response returned due to broken circuit for the HttpClient {fallbackEvent.HttpClientName}");
        return Task.CompletedTask;
    }

    public Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent)
    {
        //do something like logging
        _logger.LogInformation($"Fallback response returned due to TaskCancelledException for the HttpClient {fallbackEvent.HttpClientName}");
        return Task.CompletedTask;
    }
}
```

With the above whenever an event is triggered from any of the resilience policies on the `my-http-client` `HttpClient` there will be a log message for it.

There are overloads that enable you to have more control on how the instance that will handle the events is created. For instance:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddResiliencePolicies(
        configureOptions: options =>
        {
            options.Timeout.TimeoutInSecs = 1;
            options.Retry.RetryCount = 2;
            options.Retry.MedianFirstRetryDelayInSecs = 1;
            options.CircuitBreaker.MinimumThroughput = 10;
            options.CircuitBreaker.DurationOfBreakInSecs = 30;
            options.CircuitBreaker.SamplingDurationInSecs = 10;
            options.CircuitBreaker.FailureThreshold = 0.5;
        },
        eventHandlerFactory: provider =>
        {
            // This would be the same as using the `AddResiliencePolicies<MyResilienceEventHandler>`.
            // It's just an example of how you can control the creaton of the object handling the
            // policy events.
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MyResilienceEventHandler>();
            return new MyResilienceEventHandler(logger);
        });
```

For the majority of the cases the overload that accepts a genericy type `AddResiliencePolicies<T>` is what is more likely to be used since whatever dependencies you need to provide to the type `T` can be passed through the constructor as long as they are added to the `IServiceCollection`.
