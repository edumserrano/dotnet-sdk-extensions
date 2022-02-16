# Extensions for .NET SDK

[![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml)
[![dotnet format](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml)
[![CodeQL](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/edumserrano/dot-net-sdk-extensions/branch/main/graph/badge.svg?token=KYW77A6UV9)](https://codecov.io/gh/edumserrano/dot-net-sdk-extensions)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/edumserrano)](https://github.com/sponsors/edumserrano)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Eduardo%20Serrano-blue.svg)](https://www.linkedin.com/in/eduardomserrano/)

This repo contains extensions to help build .NET applications, using .net core 3.1 and higher, as well as extensions for unit and [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

## Installing

This repo provides two NuGet packages:

| NuGet                                                                                         |                                                                               Version                                                                                |                                Downloads                                |
| --------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :---------------------------------------------------------------------: |
| [DotNet-Sdk-Extensions](https://www.nuget.org/packages/DotNet-Sdk-Extensions)                 |             [![Nuget DotNet-Sdk-Extensions](https://img.shields.io/nuget/v/DotNet-Sdk-Extensions)](https://www.nuget.org/packages/DotNet-Sdk-Extensions)             |     ![Nuget](https://img.shields.io/nuget/dt/DotNet-Sdk-Extensions)     |
| [DotNet-Sdk-Extensions-Testing](https://www.nuget.org/packages/DotNet-Sdk-Extensions-Testing) | [![Nuget DotNet-Sdk-Extensions-Testing](https://img.shields.io/nuget/v/DotNet-Sdk-Extensions-Testing)](https://www.nuget.org/packages/DotNet-Sdk-Extensions-Testing) | ![Nuget](https://img.shields.io/nuget/dt/DotNet-Sdk-Extensions-Testing) |

Installation is performed via NuGet and you can do it using the following commands:

```
dotnet add package DotNet-Sdk-Extensions
dotnet add package DotNet-Sdk-Extensions-Testing
```

## Extensions list

The extensions available are split into two groups:

* Extensions to use on app code.
* Extensions to use when doing integration and unit tests.

For more information about each extension check its docs. You can find the link to each extension's documentation below.

### For apps

* [Eagerly validating options](/docs/configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](/docs/configuration/options-without-IOptions.md)
* Extending [Polly](https://github.com/App-vNext/Polly)
  * [Circuit breaker checker policy](/docs/polly/circuit-breaker-checker-policy.md)
  * [Add a timeout policy to an HttpClient](/docs/polly/httpclient-with-timeout-policy.md)
  * [Add a retry policy to an HttpClient](/docs/polly/httpclient-with-retry-policy.md)
  * [Add a circuit breaker policy to an HttpClient](/docs/polly/httpclient-with-circuit-breaker-policy.md)
  * [Add a fallback policy to an HttpClient](/docs/polly/httpclient-with-fallback-policy.md)
  * [Add a set of resilience policies to an HttpClient](/docs/polly/httpclient-with-resilience-policies.md)
  * [Extending the policy options validation](/docs/polly/extending-policy-options-validation.md)

### For integration tests

* [Providing test appsettings files to the test server](/docs/integration-tests/configuring-webhost.md)
* [Override configuration values on the test server](/docs/integration-tests/override-configuration-value.md)
* [Disable logs when doing integration tests](/docs/integration-tests/disable-logs-integration-tests.md)
* [Mocking HttpClient's responses in-process](/docs/integration-tests/http-mocking-in-process.md)
* [Mocking HttpClient's responses out-of-process](/docs/integration-tests/http-mocking-out-of-process.md)
* [Integration tests for HostedServices (Background Services)](/docs/integration-tests/hosted-services.md)

### For unit tests

* [Mocking HttpClient's responses for unit testing](/docs/unit-tests/http-mocking-unit-tests.md)

### Other

* [Notes on WebApplicationFactory regarding asp.net integration tests](/docs/integration-tests/web-application-factory.md)

## Debugging

The NuGet packages published include symbols generated with [sourcelink](https://github.com/dotnet/sourcelink).

For more information on how to debug the NuGet packages code from your application see:

- [Exploring .NET Core's SourceLink - Stepping into the Source Code of NuGet packages you don't own](https://www.hanselman.com/blog/exploring-net-cores-sourcelink-stepping-into-the-source-code-of-nuget-packages-you-dont-own)
- [How to Configure Visual Studio to Use SourceLink to Step into NuGet Package Source](https://aaronstannard.com/visual-studio-sourcelink-setup/).
- [Source Link - microsoft docs](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)

## Dev notes

For notes aimed at developers working on this repo or just trying to understand it go [here](/docs/dev-notes/dev-notes-main.md). It will show you how to build and run the solution among other things.

