﻿# Eagerly validating options

## :warning: Note

If using .NET 6 there is now a built in API to solve this. Please see:

- [Add Eager Options Validation: ValidateOnStart API](https://github.com/dotnet/runtime/pull/47821)
- [OptionsBuilderExtensions.ValidateOnStart<TOptions>(OptionsBuilder<TOptions>) Method](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.optionsbuilderextensions.validateonstart)

## Motivation

I want to be able to make sure the appsettings files are populated correctly and if not I want the web app to fail to start.

At the moment if you have an incorrect appsettings file(s), because for instance you forgot to populate some configuration value, the web app still starts and will work correctly until the first time that configuration value is required.

Furthermore there are ways to [decorate options classes with data validation attributes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#options-validation) but those only take effect when the options class is first instantiated which as explained before does not happen at the startup of the web app.

For more information see this GitHub issue [Developers can get immediate feedback on validation problems](https://github.com/dotnet/runtime/issues/36391).

### Issues with not having earger options validation

There are for sure many examples of situations where the lack of eager options validation causes a problem. Take for instance the following example:

* You do a change to the code and update the appsettings values in the appsettings file for environment A but forget to update it for environment B. In this case, you might end up deploying the app in environment B and not find out you have a problem until a later time. It would be preferable that the app fails to deploy because it fails to start.

## Requirements

You will have to add the [dotnet-sdk-extensions](https://www.nuget.org/packages/dotnet-sdk-extensions) nuget to your project.

## How to use

Imagine that you have an appsettings file with the following:

```json
"MyOptionsSection": {
    "SomeOption": ""
}
```

Which is represented by the typed class `MyOptions`. Notice the data annotation attribute on the `MyOptions.SomeOption` property:

```csharp
public class MyOptions
{
    [Required]
    public string SomeOption { get; set; }
}
```

If you want to make sure the `MyOptions` class is validated when the web app is starting up use the `OptionsBuilder.ValidateEagerly` extension method:

```csharp
services
    .AddOptions<MyOptions>()
    .Bind(configuration.GetSection("MyOptionsSection"))
    .ValidateDataAnnotations()
    .ValidateEagerly();
```

The way the eager validation is enforced is by creating all the instances of `T` for any `IOptions<T>` present in the `IServiceCollection` at app startup. This forces existing validation to be executed.

Note that `OptionsBuilder.ValidateEagerly` works in conjunction with dotnet's validation. In the above example we are using the `OptionsBuilder.ValidateDataAnnotations`. You could also use the `IValidateOptions` interface or implement validation at the `Startup` level with the `OptionsBuilder.Validate`. For more information see [Options Validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#options-validation) and [IValidateOptions for complex validation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#ivalidateoptions-for-complex-validation).
