# Disable logs when doing integration tests

This extension allows you to configure the log level when running integration tests.

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) but I don't want any logs to be produced.

By default when running integration tests you will see several log messages from the test server starting up as well as from the requests being made to the test server. These might clutter your output specially in build pipelines.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests). After you can disable logs as follows:

```csharp
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
                    .UseDefaultLogLevel(LogLevel.None)
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

Alternatively you can set the log level to `LogLevel.Critical` which will hide all logs except critical ones which you might be interested in seeing in the log output.

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the test [UseDefaultLogLevelDemoTests.UseDefaultLogLevelDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/UseDefaultLogLevelDemoTests.cs).

Analyse the [UseDefaultLogLevelDemoTests.UseDefaultLogLevelDemoTest test](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/Configuration/UseDefaultLogLevelDemoTests.cs) for more information on how this extension works.
