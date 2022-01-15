# Circuit breaker checker policy

## Motivation

I want to use a [Polly circuit breaker policy](https://github.com/App-vNext/Polly#circuit-breaker) or [advanced circuit breaker policy](https://github.com/App-vNext/Polly#advanced-circuit-breaker) but don't want the circuit to throw an exception when its state is open/isolated.

The idea came after reading [Reducing thrown exceptions when the circuit is broken](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken).

## Requirements

You will have to add the [DotNet-Sdk-Extensions](https://www.nuget.org/packages/DotNet-Sdk-Extensions) nuget to your project.

## How to use

### Simple example

You can augment the circuit breaker policy with the circuit breaker checker policy as follows:

```csharp
// create the circuit breaker policy
var circuitBreakerPolicy = Policy<int>
    .Handle<Exception>()
    .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
// create the circuit breaker checker policy
var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
    circuitBreakerPolicy: circuitBreakerPolicy,
    fallbackValueFactory: (circuitState, context, cancellationToken) => Task.FromResult(-1));
// create a policy that applies both the circuit breaker checker and the circuit breaker policy
var finalPolicy = Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
```

The above is just one way of using the Polly. You can integrate the circuit breaker checker policy in as many different ways as Polly provides support for. For instance you could use the [PolicyRegistry](https://github.com/App-vNext/Polly/wiki/PolicyRegistry) to achieve a similar result.

### Example with HttpClient

When [configuring an HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0) for your app you can add the circuit breaker and circuit breaker checker policy by doing the following:

```csharp
services
    .AddHttpClient("my-http-client")
    .AddHttpMessageHandler(() =>
    {
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
        var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                fallbackValueFactory: (circuitState, context, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult<HttpResponseMessage>(response);
                });
        var finalPolicy = Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
        return new PolicyHttpMessageHandler(finalPolicy);
    })
```

In this example we create a [wrapped policy](https://github.com/App-vNext/Polly#policywrap) that will first apply the circuit breaker checker policy and then the circuit breaker policy and then we add an `PolicyHttpMessageHandler` to the `HttpClient` that uses the wrapped policy.

### Example with HttpClient and PolicyRegistry

```csharp
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
    circuitBreakerPolicy: circuitBreakerPolicy,
    fallbackValueFactory: (circuitState, context, cancellationToken) =>
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        return Task.FromResult<HttpResponseMessage>(response);
    });
var registry = new PolicyRegistry
{
    {"circuit-breaker", circuitBreakerPolicy},
    {"circuit-breaker-checker", circuitBreakerCheckerPolicy}
};
services.AddPolicyRegistry(registry);

services
    .AddHttpClient("my-http-client")
    .AddPolicyHandlerFromRegistry("circuit-breaker-checker")
    .AddPolicyHandlerFromRegistry("circuit-breaker");
```

In this example we populate the `PolicyRegistry` with the circuit breaker checker policy and the circuit breaker policy and then we configure the `HttpClient` by applying the policies in the order we want.

### About Polly

Polly is a fantastic library and there are many ways to use it on your application. The above examples were just to show how the `CircuitBreakerCheckerAsyncPolicy` could be applied.

To find out more see Polly's docs:
- [Readme](https://github.com/App-vNext/Polly)
- [Wiki](https://github.com/App-vNext/Polly/wiki)
- [Polly and HttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory)