﻿# Using `T` options classes instead of `IOptions<T>`

- [Motivation](#motivation)
- [Requirements](#requirements)
- [How to use](#how-to-use)

## Motivation

I want to be able inject the options type `T` as a dependency instead of `IOptions<T>`.

The [docs for the options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) used in asp.net core apps explains how you can read configuration values and use them through your app. However, there might be cases where you do not require the advantages provided by `IOptions` and want just to use an instance of the typed configuration.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

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

Imagine that you have an appsettings file with the following:

```json
"MyOptionsSection": {
    "SomeOption": "hi"
}
```

Which is represented by the typed class `MyOptions`:

```csharp
public class MyOptions
{
    public string SomeOption { get; set; }
}
```

And now you want to take the `MyOptions` type as a dependency. As an example:

```csharp
public class SomeClass
{
    public SomeClass(SomeOption someOption)
    {

    }
}
```

To be able to do the above you can use the `OptionsBuilder.AddOptionsValue` extension method:

```csharp
services
    .AddOptions<MyOptions>()
    .Bind(configuration.GetSection("MyOptionsSection"))
    .AddOptionsValue();
```

Equivalently to the above, you can use the `IServiceCollection.AddOptionsValue` extension method:

```csharp
services.AddOptionsValue<MyOptions>(_configuration, sectionName: "MyOptionsSection");
```

In the first example you are still required to have called `IServiceCollection.AddOptions` (and optionally configure your options as desired) before using `OptionsBuilder.AddOptionsValue`. In the second example, you get a 'shortcut' way of calling it which should work for most scenarios and still allow you to further configure the options class `MyOptions` by using other `OptionsBuilder` methods. For instance:

```csharp
services
    .AddOptionsValue<MyOptions>(_configuration, sectionName: "MyOptionsSection")
    .ValidateDataAnnotations();
```

> [!NOTE]
>
> The `AddOptionsValue` extension methods add the ability to take a dependency on `SomeOption`, they don't remove remove the ability to take a dependency on `IOptions<SomeOption>`.
>
