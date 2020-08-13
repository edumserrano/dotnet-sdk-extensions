# dot-net-sdk-extensions

This repo contains extensions to use with .NET applications, mainly [asp.net core web app](https://docs.microsoft.com/en-us/aspnet/core), and its [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests). Applies only to .NET core and above.

## Extensions list

The extensions available are split into two groups:

* Extensions to use on web app main code.
* Extensions to use when doing integration tests.

For more information about each extension check its docs and demo. You can find the link to each extension's documentation below.

### For web apps

* [Eagerly validating options](/docs/configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](/docs/configuration/options-without-IOptions.md)

### For integration tests

* [Providing test appsettings files to the test server](/docs/integration-tests/configuring-webhost.md)
* [Mocking HttpClient responses](/docs/integration-tests/http-mocking.md)
* [Testing Hosted Services (Background Services)](/docs/integration-tests/hosted-services.md)

## How to run the demos

### For the web apps demos

The extensions for web apps are demoed by the `demos\AspNetCore.Extensions.Demos\AspNetCore.Extensions.Demos.csproj` project.

This project runs an asp.net core app which can demo different scenarios depending on the Startup class that is used. To chose the demo to run go to the [Program.cs](/demos/AspNetCore.Extensions.Demos/Program.cs) and make sure the `IWebHostBuilder.UseStartup` being used is the one you want.

### For the integration tests demos

The extensions for web apps are demoed by the `demos\AspNetCore.Extensions.Testing.Demos\AspNetCore.Extensions.Testing.Demos.csproj` project. Check out the tests for the extension you want.

## TODO

* Need to create tests for the extensions
* Create ci cd pipeline, add badge to readme (link to pipeline, code coverage)
* Publish nuget packages
* Setup hooks to run pipelines on pull requests
