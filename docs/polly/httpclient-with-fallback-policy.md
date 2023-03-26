# Add a fallback policy to an HttpClient

## Motivation

Every time I use an `HttpClient` I end up repeating the same [Polly](https://github.com/App-vNext/Polly) usage pattern in my projects to a set of resilience polices such as:

- retry
- circuit breaker
- timeout

When adding those policies I also end up adding a fallback policy to control what can be returned by the `HttpClient`. Eg avoid throwing a `TimeoutRejectedException` when the timeout policy triggers.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

The extension method provided `AddFallbackPolicy` is an extension to the `IHttpClientBuilder` which is what you use when [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0). This extension will adds a [fallback policy](https://github.com/App-vNext/Polly#fallback) to the `HttpClient`.

**Note** that the `AddFallbackPolicy` adds an opinionated fallback policy which is mainly meant to be used as a fallback for the policies added by the following extension methods:

- [Add a timeout policy to an HttpClient](/docs/polly/httpclient-with-timeout-policy.md)
- [Add a retry policy to an HttpClient](/docs/polly/httpclient-with-retry-policy.md)
- [Add a circuit breaker policy to an HttpClient](/docs/polly/httpclient-with-circuit-breaker-policy.md)

The fallback policy added is configured to handle exceptions and always return a type that derives from `HttpResponseMessage` and whose status code is `500`. Each derived type might contain further properties.

The fallback policy is configured to handle the following exceptions:

- `HttpRequestException`: the fallback response is a `ExceptionHttpResponseMessage`.
- `TimeoutRejectedException`: the fallback response is a `TimeoutHttpResponseMessage`.
- `BrokenCircuitException` and `IsolatedCircuitException`: the fallback response is a `CircuitBrokenHttpResponseMessage`.
- `TaskCanceledException`: the fallback response is a `AbortedHttpResponseMessage` or a `TimeoutHttpResponseMessage` if the inner exception is `TimeoutException`.

### Basic example

You can add a fallback policy by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddFallbackPolicy();
```

The above example is the simplest way to use the extension method. Note that:

- even though the example shows adding a fallback policy to a named `HttpClient` you can also add it to typed `HttpClient` because the extension method works on the `IHttpClientBuilder`.

- you can provide a class to handle the events produced by the fallback policy. See [Handling events from the fallback policy](#handling-events-from-the-fallback-policy).

### Handling events from the fallback policy

This extension method also enables you to access the events provided by Polly's fallback policy.

You can specify a class to handle the fallback events by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddFallbackPolicy<MyFallbackEventHandler>();
```

The `MyFallbackEventHandler` must implement the `IFallbackPolicyEventHandler` interface.

```csharp
public class MyFallbackEventHandler : IFallbackPolicyEventHandler
{
    private readonly ILogger<MyFallbackEventHandler> _logger;

    public MyFallbackEventHandler(ILogger<MyFallbackEventHandler> logger)
    {
        _logger = logger;
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

With the above whenever a fallback is returned from the `my-http-client` `HttpClient` there will be a log message for it.

There are overloads that enable you to have more control on how the instance that will handle the events is created. For this specic example it doesn't make much sense but could use the overload as follows:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddFallbackPolicy(provider =>
    {
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<MyFallbackEventHandler>();
        return new MyFallbackEventHandler(logger);
    });
```

For the majority of the cases the overload that accepts a genericy type `AddFallbackPolicy<T>` is what is more likely to be used since whatever dependencies you need to provide to the type `T` can be passed through the constructor as long as they are added to the `IServiceCollection`.

## Distinguish different fallback response types

With this fallback in place you reduce exceptions throw by your app. Why does this matter ? Because it helps you [avoid doing flow control with exceptions as well as reduces the performance penalty of propagating exceptions through your code](https://mattwarren.org/2016/12/20/Why-Exceptions-should-be-Exceptional/).

With this fallback policy you can now write the following:

```csharp
//httpClient is an HttpClient with the fallback policy applied
var response = await httpClient.GetAsync("/some-path");
if (response.IsSuccessStatusCode)
{
    // do something because the request was successful
}
else
{
    // do something because the request failed
}
```

In the above example you don't have to worry about handling exceptions because the fallback policy will handle them and return either an `HttpResponseMessage` or a derived type of `HttpResponseMessage` with a `500` `Internal Server Error` status code.

Furthermore if you need to differentiate the handling of failed requests depending on why it failed then you could do the following:

```csharp
//httpClient is an HttpClient with the fallback policy applied
var response = await httpClient.GetAsync("/some-path");
if (response.IsSuccessStatusCode)
{
    // do something because the request was successful
}
else
{
    // do something because the request failed
    switch (response)
    {
        case AbortedHttpResponseMessage abortedHttpResponseMessage:
            // do something because the request was aborted (TaskCancelledException)
            break;
        case CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage:
            // do something because the request failed due to the circuit being broken
            break;
        case ExceptionHttpResponseMessage exceptionHttpResponseMessage:
            // do something because the request failed due an HttpRequestException
            break;
        case TimeoutHttpResponseMessage timeoutHttpResponseMessage:
            // do something because the request timed out
            // (TimeoutRejectedException or TaskCancelledException with inner exception of TimeoutException)
            break;
        default: // default will be an instance of type HttpResponseMessage
            // do something because the request failed due to the response returned containing a failure status code. Eg 404 Not Found
            break;
    }
}
```
