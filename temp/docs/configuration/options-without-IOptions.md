# Using `T` options classes instead of `IOptions<T>`

## Motivation

I want to be able inject the options type `T` as a dependency instead of `IOptions<T>`. 

The [docs for the options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) used in asp.net core apps explains how you can read configuration values and use them through your app. However, there might be cases where you do not require the advantages provided by `IOptions` and want just to use an instance of the typed configuration.

## Requirements

You will have to add the [DotNet-Sdk-Extensions](https://www.nuget.org/packages/DotNet-Sdk-Extensions) nuget to your project.

## How to use

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

Also note that this extension method just added the ability to take a dependency on `SomeOption`, it didn't remove the ability to take a dependency on `IOptions<SomeOption>`.
