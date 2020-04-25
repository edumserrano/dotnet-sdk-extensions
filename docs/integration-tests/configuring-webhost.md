# Configuring your WebHost when doing integration tests

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) and I need to be able to control the configuration for the tests.

The main configuration values I want to be able to control are the ones read from the appsettings files.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

Once you have your test ready configure the appsettings files to be used on the test by using the `IWebHostBuilder.AddTestConfiguration` extension method.

Assume that you have a directory in the root of the test project named *AppSettings* and that inside you have two files:

* appsettings.json
* appsettings.Default.json

Given this you can do a test as shown by the DemoTest below:

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

Note that the location of where the test appsettings files are can be configured. See below for more information.

### Configure the location of the test appsettings files

You can configure the location of where the appsettings files are by using an overload of `IWebHostBuilder.AddTestConfiguration` extension method as follows:

```
.AddTestConfiguration(options => options.AppSettingsDir = "AppSettings", "appsettings.json", "appsettings.Default.json")
```

The value set for `TestConfigurationOptions.AppSettingsDir` will be used as a relative path to the current directory (directory where the tests will be executed).

Make sure you set your test appsetting files to be copied to the output.

### How does it work with the appsettings files outside the test project

When using `IWebHostBuilder.AddTestConfiguration` any existing `Microsoft.Extensions.Configuration.JsonJsonConfigurationSource` instances are removed from the `IServiceCollection` which will be used by the `WebApplicationFactory` when starting the test server.

This means that any appsettings file that would normally be loaded, the actual ones where you usually split them by environment, are not loaded on the tests.

This is done to avoid acidentally loading some configuration values. It has the small downside you need to load appsettings files that between them have the required configuration values to run the given test scenario.

Also note that by default the appsettings that are loaded are based on the environment on which the WebHost is running. As an example, by default when debbuging the app the *ASPNETCORE_ENVIRONMENT* environment variable is set to *Development* and the appsettings that are loaded are *appsettings.json* and *appsettings.Development.json*. 

The `IWebHostBuilder.AddTestConfiguration` extension method does not take the environment into consideration when loading test appsettings. Following the example in the [DemoTest](#how-to-use), when doing:

```
.AddTestConfiguration("appsettings.json", "appsettings.Default.json")
```

Both the *appsettings.json* and *appsettings.Default.json* are loaded as configuration sources. You don't need to make sure the app is running with the *ASPNETCORE_ENVIRONMENT* environment variable set to *Default*.

## How to run the demo

The demo for this extension is represented by a test class.

* Go to the project `/demos/AspNetCore.Extensions.Testing.Demos/AspNetCore.Extensions.Testing.Demos.csproj`
* Run the test `ConfiguringWebHostDemoTests.ConfiguringWebHostDemoTest`