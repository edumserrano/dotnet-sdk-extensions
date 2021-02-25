# Providing test appsettings files to the test server

This extension allows you to pass configuration files to use during integration tests.

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) and I need to be able to control the configuration for the tests.

The main configuration values I want to be able to control are the ones read from the appsettings files.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

After, configure the appsettings files to be used on the test by using the `IWebHostBuilder.AddTestConfiguration` extension method.

Let's say that you create a directory in the root of the test project named *AppSettings* and that inside you have two files:

* appsettings.json
* appsettings.Default.json

Given this you can do a test as shown by the DemoTest method below:

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
                    .AddTestConfiguration("appsettings.json", "appsettings.Default.json")
                    .ConfigureTestServices(services =>
                    {
                        // inject mocks for any other services
                    });
            })
            .CreateClient();

        // do some calls to your app via the httpClient and then some asserts
    }
}
```

Note that all of the code in the example above is equal to the demos explained on the [official docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) except for the addition of the `.AddTestConfiguration("appsettings.json", "appsettings.Default.json")` line.

Also note that the location of where the test appsettings files are can be configured. See below for more information.

## Configure the location of the test appsettings files

You can configure the location of where the appsettings files are by using an overload of `IWebHostBuilder.AddTestConfiguration` extension method as follows:

```
.AddTestConfiguration(options => options.AppSettingsDir = "AppSettings", "appsettings.json", "appsettings.Default.json")
```

The value set for `TestConfigurationOptions.AppSettingsDir` will be used as a relative path to the current directory (directory where the tests will be executed). It defaults to `AppSettings`.

Make sure you set your test appsetting files to be copied to the output.

## How does it work with the appsettings files outside the test project (ie the real ones on the main project)

When using `IWebHostBuilder.AddTestConfiguration` any existing `Microsoft.Extensions.Configuration.JsonJsonConfigurationSource` instances are removed from the `IServiceCollection` which will be used by the `WebApplicationFactory` when starting the test server.

This means that any appsettings file that would normally be loaded, the actual ones your app uses when running, are not loaded on the tests.

This is done to avoid acidentally loading some configuration values. It has the small downside that you need to load appsettings files that between them have the required configuration values to run the given test scenario instead of relying on the appsettings used by your app and just overwriting the required bits.

Also note that by default the appsettings that are loaded are based on the environment on which the WebHost is running. As an example, by default when debbuging the app the `ASPNETCORE_ENVIRONMENT` environment variable is set to `Development` and the appsettings that are loaded are `appsettings.json` and `appsettings.Development.json`. 

The `IWebHostBuilder.AddTestConfiguration` extension method does not take the environment into consideration when loading test appsettings. Following the example in the [DemoTest](#how-to-use), when doing:

```
.AddTestConfiguration("appsettings.json", "appsettings.Default.json")
```

Both the `appsettings.json` and `appsettings.Default.json` are loaded as configuration sources. You don't need to make sure the app is running with the `ASPNETCORE_ENVIRONMENT` environment variable set to `Default`.

## Notes

This extension method works for both `IWebHostBuilder` and `IHostBuilder`.

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the test [ConfiguringWebHostDemoTests.ConfiguringWebHostDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/ConfiguringWebHostDemoTests.cs).

Analyse the [ConfiguringWebHostDemoTests.ConfiguringWebHostDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/ConfiguringWebHostDemoTests.cs) for more information on how this extension works.
