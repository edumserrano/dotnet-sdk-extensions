# Add a timeout policy to an HttpClient

## Motivation

Every time I use an `HttpClient` I end up repeating the same [Polly](https://github.com/App-vNext/Polly) usage pattern in my projects to add a timeout policy.

Plus, at times I want to have the values for configuring the timeout policy read from the `appsettings.json` which further increases the code I keep repeating.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

The extension method provided `AddTimeoutPolicy` is an extension to the `IHttpClientBuilder` which is what you use when [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0).

This extension will add a [timeout policy](https://github.com/App-vNext/Polly#timeout) to the `HttpClient`.

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

You can add a timeout policy by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy(options =>
    {
        options.TimeoutInSecs = 1;
    });
```

The above example is the simplest way to use the extension method. Note that:

- even though the example shows adding a timeout policy to a named `HttpClient` you can also add it to typed `HttpClient` because the extension method works on the `IHttpClientBuilder`.

- the configuration of the policy's options is done inline but the extension method is also integrated with the all the ecosystem around the [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0) in dotnet core, such as the possibility of binding the options values from the `appsettings`. See [Binding appsettings values to the timeout policy options](#binding-appsettings-values-to-the-timeout-policy-options).

- you can provide a class to handle the events produced by the timeout policy. See [Handling events from the timeout policy](#handling-events-from-the-timeout-policy).

### TimeoutOptions

The `TimeoutOptions` provides the following configuration options for the timeout policy:

- `TimeoutInSecs`: Timeout value in seconds. Must be a value between `double.Epsilon` and `double.MaxValue`. You can represent values smaller than 1 second by using a decimal number such as 0.1 which would mean a timeout of 100 milliseconds.

If you want to [bind the configuration from the appsettings](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#bind-hierarchical-configuration) remember that the name of the key in the appsettings must match the property names of the `TimeoutOptions` for the bind to work.

### Binding appsettings values to the timeout policy options

Imagine that you have an appsettings file with the following:

```json
"MyHttpClient": {
    "TimeoutInSecs": "1"
}
```

You can add a timeout policy that is configured from the values on the appsettings file by doing the following:

```csharp
services
    .AddHttpClientTimeoutOptions("my-timeout-options")
    .Bind(Configuration.GetSection("MyHttpClient"));

services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy("my-timeout-options");
```

The `services.AddHttpClientTimeoutOptions` adds a named options of type `TimeoutOptions` and returns an instance of `OptionsBuilder<TimeoutOptions>`, which means you can now use any of the methods provided by dotnet to configure it such as for example:

- [Options validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-validation)
- [Options post-configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#options-post-configuration)

### Handling events from the timeout policy

This extension method also enables you to access the events provided by Polly's timeout policy.

You can specify a class to handle the timeout events by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy<MyTimeoutEventHandler>(options =>
    {
        options.TimeoutInSecs = 1;
    });
```

The `MyTimeoutEventHandler` must implement the `ITimeoutPolicyEventHandler` interface.

```csharp
public class MyTimeoutEventHandler : ITimeoutPolicyEventHandler
{
    private readonly ILogger<MyTimeoutEventHandler> _logger;

    public MyTimeoutEventHandler(ILogger<MyTimeoutEventHandler> logger)
    {
        _logger = logger;
    }

    public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
    {
        //do something like logging
        _logger.LogInformation($"A timeout has occurred on the HttpClient {timeoutEvent.HttpClientName}");
        return Task.CompletedTask;
    }
}
```

With the above whenever a timeout occurs on the `my-http-client` `HttpClient` there will be a log message for it.

There are overloads that enable you to have more control on how the instance that will handle the events is created. For instance:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy(
        configureOptions: options =>
        {
            options.TimeoutInSecs = 1;
        },
        eventHandlerFactory: provider =>
        {
            // This would be the same as using the `AddTimeoutPolicy<MyTimeoutEventHandler>`.
            // It's just an example of how you can control the creaton of the object handling the
            // policy events.
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MyTimeoutEventHandler>();
            return new MyTimeoutEventHandler(logger);
        });
```

For the majority of the cases the overload that accepts a genericy type `AddTimeoutPolicy<T>` is what is more likely to be used since whatever dependencies you need to provide to the type `T` can be passed through the constructor as long as they are added to the `IServiceCollection`.
