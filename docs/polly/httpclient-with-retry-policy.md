# Add a retry policy to an HttpClient

- [Motivation](#motivation)
- [Requirements](#requirements)
- [How to use](#how-to-use)
  - [Basic example](#basic-example)
  - [RetryOptions](#retryoptions)
  - [Binding appsettings values to the retry policy options](#binding-appsettings-values-to-the-retry-policy-options)
  - [Handling events from the retry policy](#handling-events-from-the-retry-policy)

## Motivation

Every time I use an `HttpClient` I end up repeating the same [Polly](https://github.com/App-vNext/Polly) usage pattern in my projects to add a retry policy.

Plus, at times I want to have the values for configuring the retry policy read from the `appsettings.json` which further increases the code I keep repeating.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

The extension method provided `AddRetryPolicy` is an extension to the `IHttpClientBuilder` which is what you use when [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0).

This extension will add the retry policy provided by [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry) to the `HttpClient`.
This policy was chosen because of its more advanced jitter support. For more info see:

- [Retry with jitter](https://github.com/App-vNext/Polly/wiki/Retry-with-jitter)
- [Wait and Retry with Jittered Back-off](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-jittered-back-off)

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

You can add a retry policy by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddRetryPolicy(options =>
    {
        options.RetryCount = 2;
        options.MedianFirstRetryDelayInSecs = 1;
    });
```

The above example is the simplest way to use the extension method. Note that:

- even though the example shows adding a retry policy to a named `HttpClient` you can also add it to typed `HttpClient` because the extension method works on the `IHttpClientBuilder`.

- the configuration of the policy's options is done inline but the extension method is also integrated with the all the ecosystem around the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0) in dotnet core, such as the possibility of binding the options values from the `appsettings`. See [Binding appsettings values to the retry policy options](#binding-appsettings-values-to-the-retry-policy-options).

- you can provide a class to handle the events produced by the retry policy. See [Handling events from the retry policy](#handling-events-from-the-retry-policy).

### RetryOptions

The `RetryOptions` provides the following configuration options for the retry policy:

- `RetryCount`: maximum number of retries. Must be a value between zero and `int.MaxValue`.
- `MedianFirstRetryDelayInSecs`:  median delay to target before the first retry. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean 100 milliseconds.

If you want to [bind the configuration from the appsettings](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#bind-hierarchical-configuration) remember that the name of the key in the appsettings must match the property names of the `RetryOptions` for the bind to work.

### Binding appsettings values to the retry policy options

Imagine that you have an appsettings file with the following:

```json
"MyHttpClient": {
    "RetryCount": "2",
    "MedianFirstRetryDelayInSecs": "1",
}
```

You can add a retry policy that is configured from the values on the appsettings file by doing the following:

```csharp
services
    .AddHttpClientRetryOptions("my-retry-options")
    .Bind(Configuration.GetSection("MyHttpClient"));

services
    .AddHttpClient("my-http-client")
    .AddRetryPolicy("my-retry-options");
```

The `services.AddHttpClientRetryOptions` adds a named options of type `RetryOptions` and returns an instance of `OptionsBuilder<RetryOptions>`, which means you can now use any of the methods provided by dotnet to configure it such as for example:

- [Options validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-validation)
- [Options post-configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-post-configuration)

### Handling events from the retry policy

This extension method also enables you to access the events provided by Polly's retry policy.

You can specify a class to handle the retry events by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddRetryPolicy<MyRetryEventHandler>(options =>
    {
        options.RetryCount = 2;
        options.MedianFirstRetryDelayInSecs = 1;
    });
```

The `MyRetryEventHandler` must implement the `IRetryPolicyEventHandler` interface.

```csharp
public class MyRetryEventHandler : IRetryPolicyEventHandler
{
    private readonly ILogger<MyRetryEventHandler> _logger;

    public MyRetryEventHandler(ILogger<MyRetryEventHandler> logger)
    {
        _logger = logger;
    }

    public Task OnRetryAsync(RetryEvent retryEvent)
    {
        //do something like logging
        _logger.LogInformation($"Retry {retryEvent.RetryNumber} out of {retryEvent.RetryOptions.RetryCount} for HttpClient {retryEvent.HttpClientName}");
        return Task.CompletedTask;
    }
}
```

With the above whenever a retry occurs on the `my-http-client` `HttpClient` there will be a log message for it.

There are overloads that enable you to have more control on how the instance that will handle the events is created. For instance:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddRetryPolicy(
        configureOptions: options =>
        {
            options.RetryCount = 2;
            options.MedianFirstRetryDelayInSecs = 1;
        },
        eventHandlerFactory: provider =>
        {
            // This would be the same as using the `AddRetryPolicy<MyRetryEventHandler>`.
            // It's just an example of how you can control the creaton of the object handling the
            // policy events.
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MyRetryEventHandler>();
            return new MyRetryEventHandler(logger);
        });
```

For the majority of the cases the overload that accepts a genericy type `AddRetryPolicy<T>` is what is more likely to be used since whatever dependencies you need to provide to the type `T` can be passed through the constructor as long as they are added to the `IServiceCollection`.
