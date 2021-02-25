# Override configuration values on the test server

This extension allows you to do set configuration values for integration tests.

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) and I need to be able to control the configuration for the tests.

In [Providing test appsettings files to the test server](./configuring-webhost.md) we show a way to provide entire appsettings values to the configuration of the test server. However, in some cases you might want to provide only some configuration values or provide the full appsettings but override some values.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests). After you can set configuration values as follows:

```
public class ConfigurationDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _webApplicationFactory;

    public ConfigurationDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task DemoTest()
    {
        var httpClient = _webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder
                    .UseConfigurationValue(key: "SomeConfigOption", value: "some-option-value")
                    .ConfigureTestServices(services =>
                    {
                        // inject mocks for any other services
                    });
            })
            .CreateClient();
        
        // rest of test
    }
}
```

You can call the method several times to configure multiple values.

Optionally you can use the method provided in [Providing test appsettings files to the test server](./configuring-webhost.md) and then override some configuration values:

```
public class ConfigurationDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _webApplicationFactory;

    public ConfigurationDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task DemoTest()
    {
        var httpClient = _webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder
                    .AddTestConfiguration("appsettings.json")
                    .UseConfigurationValue(key: "SomeOption", value: "some-option-value")
                    .ConfigureTestServices(services =>
                    {
                        // inject mocks for any other services
                    });
            })
            .CreateClient();

        // rest of test
    }
}
```

## Hierarchical configuration values

You can also set the values for hierarchical configuration by using the [`:` separator](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#configuration-keys-and-values).

For instance, if we have the following `appsettings.json`:

```
"MyOptionsSection": {
    "SomeOption": "Hello from configuration"
  }
```

You can set the configuration value for `SomeOption` by calling:

```
builder.UseConfigurationValue(key: "MyOptionsSection:SomeOption", value: "some-option-value")
```

## Alternatives

## `IWebHostBuilder.SetSetting`

If you are using a `IWebHostBuilder` then you can make use of the `IWebHostBuilder.SetSetting` as follows:

```
public class ConfigurationDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _webApplicationFactory;

    public ConfigurationDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task DemoTest()
    {
        var httpClient = _webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.SetSetting(key: "SomeOption", value: "some-option-value")
                builder.ConfigureTestServices(services =>
                {
                    // inject mocks for any other services
                });
            })
            .CreateClient();

        // rest of test
    }
}
```

This method does not exist on the `IHostBuilder`.

## `IServiceCollection.PostConfigure`

If you are using typed options classes you can also override the configuration values on those types by using the [IServiceCollection.PostConfigure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#options-post-configuration).

Let's say you have an options classed defined by the type `MyOptions` as follows:

```
public class MyOptions
{
    public string SomeOption { get; set; }
}
```

And that you are [binding the `MyOptions` type in the `Startup` class](https://docs.microsoft.com/en-us/dotnet/core/extensions/options). If you want to set the value of the `MyOptions.SomeOption` for an integration test you can do as follows:


```
public class ConfigurationDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _webApplicationFactory;

    public ConfigurationDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task DemoTest()
    {
        var httpClient = _webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.PostConfigure<MyOptions>(myOptions =>
                    {
                        myOptions.SomeOption = "configured-value-in-test";
                    });
                    // inject mocks for any other services
                });
            })
            .CreateClient();

        // rest of test
    }
}
```

## Notes

This extension method works for both `IWebHostBuilder` and `IHostBuilder`.

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the test [UseConfigurationValueDemoTests.UseConfigurationValueDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/UseConfigurationValueDemoTests.cs).

Analyse the [UseConfigurationValueDemoTests.UseConfigurationValueDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/UseConfigurationValueDemoTests.cs) for more information on how this extension works.
