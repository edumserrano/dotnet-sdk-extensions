# Add a circuit breaker policy to an HttpClient

- [Motivation](#motivation)
- [Requirements](#requirements)
- [How to use](#how-to-use)
  - [Basic example](#basic-example)
  - [CircuitBreakerOptions](#circuitbreakeroptions)
  - [Binding appsettings values to the circuit breaker policy options](#binding-appsettings-values-to-the-circuit-breaker-policy-options)
  - [Handling events from the circuit breaker policy](#handling-events-from-the-circuit-breaker-policy)
  - [Note about the circuit breaker checker policy](#note-about-the-circuit-breaker-checker-policy)

> [!IMPORTANT]
>
> `.NET 8` now brings better support for adding resilience to `HttpClient`. See [Add resilience to an HTTP client](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli#add-resilience-to-an-http-client) and [Building resilient cloud services with .NET 8 | .NET Conf 2023](https://www.youtube.com/watch?v=BDZpuFI8mMM&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=16).
>
> You should consider adopting the new `.NET 8` API instead of using the one presented here.

## Motivation

Every time I use an `HttpClient` I end up repeating the same [Polly](https://github.com/App-vNext/Polly) usage pattern in my projects to add a circuit breaker policy.

Plus, at times I want to have the values for configuring the circuit breaker policy read from the `appsettings.json` which further increases the code I keep repeating.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

The `AddCircuitBreakerPolicy` method is an extension method to the `IHttpClientBuilder` which is what you use when [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0).

This extension will add a [circuit breaker policy](https://github.com/App-vNext/Polly#advanced-circuit-breaker) wrapped with a [circuit breaker checker policy](/docs/polly/circuit-breaker-checker-policy.md) to the `HttpClient`.

> [!NOTE]
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

You can add a circuit breaker policy by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddCircuitBreakerPolicy(options =>
    {
        options.FailureThreshold = 0.5;      // break on >=50% actions result in failures
        options.SamplingDurationInSecs = 10; // over any 10 second period
        options.MinimumThroughput = 8;       // provided at least 8 actions in the 10 second period
        options.DurationOfBreakInSecs = 30;  // break for 30 seconds
    });
```

The above example is the simplest way to use the extension method. Note that:

- even though the example shows adding a circuit breaker policy to a named `HttpClient` you can also add it to typed `HttpClient` because the extension method works on the `IHttpClientBuilder`.

- the configuration of the policy's options is done inline but the extension method is also integrated with the all the ecosystem around the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0) in dotnet core, such as the possibility of binding the options values from the `appsettings`. See [Binding appsettings values to the circuit breaker policy options](#binding-appsettings-values-to-the-circuit-breaker-policy-options).

- you can provide a class to handle the events produced by the circuit breaker policy. See [Handling events from the circuit breaker policy](#handling-events-from-the-circuit-breaker-policy).

### CircuitBreakerOptions

The `CircuitBreakerOptions` provides the following configuration options for the circuit breaker policy:

- `FailureThreshold`: failure threshold at which the circuit will break, eg 0.5 represents breaking if 50% or more of actions result in a handled failure. Must be a value between `double.Epsilon` and 1.
- `SamplingDurationInSecs`:  duration of the timeslice over which failure ratios are assessed. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.
- `MinimumThroughput`: this many actions or more must pass through the circuit in the timeslice for statistics to be considered significant and the circuit-breaker to come into action. Must be a value between 2 and `int.MaxValue`.
- `DurationOfBreakInSecs`: duration the circuit will stay open before resetting. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.

If you want to [bind the configuration from the appsettings](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#bind-hierarchical-configuration) remember that the name of the key in the appsettings must match the property names of the `CircuitBreakerOptions` for the bind to work.

### Binding appsettings values to the circuit breaker policy options

Imagine that you have an appsettings file with the following:

```json
"MyHttpClient": {
    "FailureThreshold": 0.5,
    "SamplingDurationInSecs": 10,
    "MinimumThroughput": 8,
    "DurationOfBreakInSecs": 30
}
```

You can add a circuit breaker policy that is configured from the values on the appsettings file by doing the following:

```csharp
services
    .AddHttpClientCircuitBreakerOptions("my-circuit-breaker-options")
    .Bind(Configuration.GetSection("MyHttpClient"));

services
    .AddHttpClient("my-http-client")
    .AddCircuitBreakerPolicy("my-circuit-breaker-options");
```

The `services.AddHttpClientCircuitBreakerOptions` adds a named options of type `CircuitBreakerOptions` and returns an instance of `OptionsBuilder<CircuitBreakerOptions>`, which means you can now use any of the methods provided by dotnet to configure it such as for example:

- [Options validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-validation)
- [Options post-configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-post-configuration)

### Handling events from the circuit breaker policy

This extension method also enables you to access the events provided by Polly's circuit breaker policy.

You can specify a class to handle the circuit breaker events by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddCircuitBreakerPolicy<MyCircuitBreakerEventHandler>(options =>
    {
        options.FailureThreshold = 0.5;
        options.SamplingDurationInSecs = 10;
        options.MinimumThroughput = 8;
        options.DurationOfBreakInSecs = 30;
    });
```

The `MyCircuitBreakerEventHandler` must implement the `ICircuitBreakerPolicyEventHandler` interface.

```csharp
public class MyCircuitBreakerEventHandler : ICircuitBreakerPolicyEventHandler
{
    private readonly ILogger<MyCircuitBreakerEventHandler> _logger;

    public MyCircuitBreakerEventHandler(ILogger<MyCircuitBreakerEventHandler> logger)
    {
        _logger = logger;
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
}
```

With the above whenever a circuit breaker event occurs on the `my-http-client` `HttpClient` there will be a log message for it.

There are overloads that enable you to have more control on how the instance that will handle the events is created. For instance:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddCircuitBreakerPolicy(
        configureOptions: options =>
        {
            options.FailureThreshold = 0.5;
            options.SamplingDurationInSecs = 10;
            options.MinimumThroughput = 8;
            options.DurationOfBreakInSecs = 30;
        },
        eventHandlerFactory: provider =>
        {
            // This would be the same as using the `AddCircuitBreakerPolicy<MyCircuitBreakerEventHandler>`.
            // It's just an example of how you can control the creaton of the object handling the
            // policy events.
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MyCircuitBreakerEventHandler>();
            return new MyCircuitBreakerEventHandler(logger);
        });
```

For the majority of the cases the overload that accepts a genericy type `AddCircuitBreakerPolicy<T>` is what is more likely to be used since whatever dependencies you need to provide to the type `T` can be passed through the constructor as long as they are added to the `IServiceCollection`.

### Note about the circuit breaker checker policy

The `AddCircuitBreakerPolicy` extension method adds a [circuit breaker checker policy](/docs/polly/circuit-breaker-checker-policy.md) that is evaluated before the circuit breaker policy.

When the circuit's state is open/isolated the circuit breaker checker policy avoids an exception being thrown and returns a instance of `CircuitBrokenHttpResponseMessage` which derives from `HttpResponseMessage` and has a status code 500.

If you want to handle the `CircuitBrokenHttpResponseMessage` type returned, here's an example you can consider:

```csharp
//httpClient is an HttpClient with a circuit breaker and a circuit breaker check policies applied
var response = await httpClient.GetAsync("/some-path");
if(response is CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage)
{
    // do something because the request failed due to the circuit being broken
}

if (response.IsSuccessStatusCode)
{
    // do something because the request was successful
}
else
{
    // do something because the request failed
}
```
