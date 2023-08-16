# Extending the policy options validation

All of the options from the following extension methods have a default validation that can be extended:

- [Add a timeout policy to an HttpClient](/docs/polly/httpclient-with-timeout-policy.md#timeoutoptions)
- [Add a retry policy to an HttpClient](/docs/polly/httpclient-with-retry-policy.md#retryoptions)
- [Add a circuit breaker policy to an HttpClient](/docs/polly/httpclient-with-circuit-breaker-policy.md#circuitbreakeroptions)

Let's see how we can extend the validation of the `TimeoutOptions` from the [`AddTimeoutPolicy` extension method](/docs/polly/httpclient-with-timeout-policy.md). The same can be applied to the options of any of the other extension methods.

> **Note**
>
> the variable `services` in the examples below is of type `IServiceCollection`. On the default template
> for a Web API you can access it via `builder.services`. Example:
>
> var builder = WebApplication.CreateBuilder(args); </br>
> builder.Services.AddControllers();
>

## Extend validation inline

The `TimeoutOptions` has a default validation but you can extend it as follows:

```csharp
services
    .AddHttpClientTimeoutOptions("my-timeout-options")
    .Bind(Configuration.GetSection("MyHttpClient"))
    .Validate(options => options.TimeoutInSecs is >= 2 and <= 4); // extend the default validation

services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy("my-timeout-options");
```

With the above even though the default validation only required the `TimeoutInSecs` to be a positive number, it's now further restricted to a number between 2 and 4.

## Extend validation with the `IValidateOptions` interface

You can extend the validation in any way provided by dotnet. For instance, you could also extend the validation using the [`IValidateOptions` interface](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0#ivalidateoptions-for-complex-validation) instead.

You would start by implementing the `IValidateOptions` interface for the `TimeoutOptions` type:

```csharp
internal class MyTimeoutOptionsValidation : IValidateOptions<TimeoutOptions>
{
    public ValidateOptionsResult Validate(string name, TimeoutOptions options)
    {
        if (options.TimeoutInSecs is >= 2 or <= 4)
        {
            return ValidateOptionsResult.Success;
        }
        return ValidateOptionsResult.Fail($"{nameof(options.TimeoutInSecs)} must be a value between 2 and 4");
    }
}
```

Then you add the validation to the `IServiceCollection`:

```csharp
// with this when an instance of TimeoutOptions is requested the MyTimeoutOptionsValidation.Validate method will execute
services.AddSingleton<IValidateOptions<TimeoutOptions>, MyTimeoutOptionsValidation>();
```

And finally you add the timeout policy:

```csharp
services
    .AddHttpClientTimeoutOptions("my-timeout-options")
    .Bind(Configuration.GetSection("MyHttpClient"));

services
    .AddHttpClient("my-http-client")
    .AddTimeoutPolicy("my-timeout-options");
```
