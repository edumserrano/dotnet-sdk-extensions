# dot-net-sdk-extensions

This repo contains extensions to use with .NET applications, mainly [asp.net core web apps](https://docs.microsoft.com/en-us/aspnet/core), and respective [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests). Applies only to .NET core and above.

## Extensions list

The extensions available are split into two groups:

* Extensions to use on app main code.
* Extensions to use when doing integration tests.

For more information about each extension check its docs and demo. You can find the link to each extension's documentation below.

### For apps

* [Eagerly validating options](/docs/configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](/docs/configuration/options-without-IOptions.md)

### For integration tests

* [Providing test appsettings files to the test server](/docs/integration-tests/configuring-webhost.md)
* [Mocking HttpClient responses](/docs/integration-tests/http-mocking.md)
* [Testing Hosted Services (Background Services)](/docs/integration-tests/hosted-services.md)

### Other

* [Notes on WebApplicationFactory regarding integration tests](/docs/integration-tests/web-application-factory.md)

## How to run the demos

### For the app demos

The extensions for apps are demoed by the `demos\DotNet.Sdk.Extensions.Demos\DotNet.Sdk.Extensions.Demos.csproj` project.

This project runs an `asp.net core app` which can demo different scenarios. For more information on how to run a specific demo see the documentation for the desired extension.

### For the integration tests demos

The extensions for web apps are demoed by the `demos\DotNet.Sdk.Extensions.Testing.Demos\DotNet.Sdk.Extensions.Testing.Demos.csproj` project. Check out the tests for the extension you want. For more information on how to run a specific demo see the documentation for the desired extension.

## TODO

* get lambda extensions 
* investigate serilog logging test extensions for integration tests
* Create ci cd pipeline, add badge to readme (link to pipeline, code coverage)
* Publish nuget packages
* Setup hooks to run pipelines on pull requests


## Notes

If you have problems with SSL certificates when running the demos or tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
