# Using `T` options classes instead of `IOptions<T>`

## Motivation

I want to be able inject the options type `T` as a dependency instead of `IOptions<T>`. 

The [docs for the options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) used in asp.net core apps explains how you can read configuration values and use them through your app. However, there might be cases where you do not require the advantages provided by `IOptions` and want just to use an instance of the typed configuration.

## How to use

Imagine that you have an appsettings file with the following:

```
"MyOptionsSection": {
	"SomeOption": "hi"
}
```

Which is represented by the typed class `MyOptions`:

```
public class MyOptions
{
	public string SomeOption { get; set; }
}
```

And now you want to take the `MyOptions` type as a dependency. As an example:

```
public class SomeClass
{
	public SomeClass(SomeOption someOption)
	{

	}
}
```

To be able to do the above you can use `OptionsBuilder.AddOptionsValue`:

```
services
	.AddOptions<MyOptions>()
	.Bind(configuration.GetSection("MyOptionsSection"))
	.AddOptionsValue();
```

Note that you are still required to have called `IServiceCollection.AddOptions` (and optionally configure your options as desired) before using `OptionsBuilder.AddOptionsValue`.

Also note that this just added the hability to take a dependency on `SomeOption`, it didn't remove the hability to take a dependency on `IOptions<SomeOption>`.

## How to run the demo

The demo for this extension is represented by a web app.

* From Visual Studio, set the `demos\DotNet.Sdk.Extensions.Demos\DotNet.Sdk.Extensions.Demos.csproj` project as the Startup Project
* Update the `launchSettings.json` to set the demo to the options value by going to `demos\DotNet.Sdk.Extensions.Demos\Properties\launchSettings.json` and setting the `commandLineArgs` value to `-d options-value`.
* Run the project
* Browse to https://localhost:5001/ and the following message should be displayed:
  
```
appsettings says: Hello from typed configuration
```

Analyse the [StartupOptionsValue class](/demos/DotNet.Sdk.Extensions.Demos/Options/OptionsValue/StartupOptionsValue) for more information on how this demo works.