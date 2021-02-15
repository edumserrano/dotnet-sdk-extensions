# Extensions and guides for .NET SDK

This repo contains extensions to use with .NET applications, using .net core 3.1 and higher, as well as extensions for unit and [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

It also contains guides on scenarios around building apps using .NET SDK. These guides are for situations where an extension is not merited but some documentation on how to accomplish the task is.

## Documentation

For documentation about the extensions and guides availables go [here](/docs/docs-main.md).

## GitHub Actions

| Worflow                   |      Status and link      |
|---------------------------|:-------------------------:|
| [build-demos](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/build-demos.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Build%20demos/badge.svg) |
| [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/nuget-publish.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Publish%20Nuget%20packages/badge.svg) |

## Installing

This repo provides two NuGet packages:

- [DotNet-Sdk-Extensions](https://www.nuget.org/packages/DotNet-Sdk-Extensions)
- [DotNet-Sdk-Extensions-Testing](https://www.nuget.org/packages/DotNet-Sdk-Extensions-Testing)

Installation is performed via NuGet and you can do it using the following commands:

```
dotnet add package DotNet-Sdk-Extensions
dotnet add package DotNet-Sdk-Extensions-Testing
```

## Building

### Using Visual Studio

1) Clone the repo and open one of the solution files:
   - **DotNet.Sdk.Extensions.sln:** source for the extensions.
   - **DotNet.Sdk.Extensions.Demos.sln:** demo projects for the extensions and the guides.

2) Press build on Visual Studio.

### Using dotnet CLI

1) Clone the repo and browse to the directory using your favorite shell.

2) Run:
   - **`dotnet build DotNet.Sdk.Extensions.sln`:** to build the source for the extensions.
   - **`dotnet build DotNet.Sdk.Extensions.Demos.sln`:** to build the demos for the extensions and the guides.

## License

This project is licensed under the [MIT license](./LICENSE).

## TODO


* update readme and pipeline
  * fix build warnings 
  * code coverage ? publish to github action? can I publish the nuget as well just for easy download?
  * add note about sourcelink being enabled and how to debug nuget package code
  * explain that the snupkg is published but that the pdb is also part of the nuget to enable debugging in two ways (explain why, one is much slower and requires nuget symbols)
  * add doc about github action (how the api key to push nuget is stored and that it needs to be refreshed in 1year) https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package#:~:text=Sign%20into%20your%20nuget.org,%2C%20select%20Select%20Scopes%20%3E%20Push.
  * move packages from alpha to stable and update it on demo sln
  * any readme missing? 
  
* guide about dotnet test solution with code coverage
  * solution wide code coverage: https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Examples/MSBuild/MergeWith/HowTo.md
  * note that %2c is used instead of comma: "json%2copencover"="json,opencover". For more information see: https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md#note-for-powershell--vsts-users
  * note that the mergewith param needs to be an absolute directory or else the reports won't be merged correctly. This is because the mergewith param when used as a relative directory is relative to the current test project being executed so when you have multiple test projects, unless they are all in the same level relative to each other than the mergewith directory will be different from one test proj to another.
  * explain how to get code coverage and a report locally

```
  
          dotnet test ${{parameters.appSolutionPath}} `
            --results-directory "$(Build.SourcesDirectory)/tests/test-results" `
            --logger trx `
            /p:CollectCoverage=true `
            /p:Include="${{parameters.testCoverageInclude}}" `
            /p:CoverletOutput="$(Build.SourcesDirectory)/CodeCoverage/" `
            /p:MergeWith="$(Build.SourcesDirectory)/CodeCoverage/coverage.json" `
            /p:CoverletOutputFormat="json%2ccobertura" `
            -m:1
```
```
dotnet test RosettaProxy.sln `
    --results-directory "$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results")" `
    --logger trx `
    /p:CollectCoverage=true `
    /p:Include="[LexisNexis.Rosetta.Proxy]*%2c[LexisNexis.Rosetta.Core]*" `
    /p:CoverletOutput="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/")" `
    /p:MergeWith="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.json")" `
    /p:CoverletOutputFormat="json%2copencover" `
    -m:1
```

### Code coverage

1. Browse to the repo's root directory
2. Delete the temp tests directory. Optional but prevents any previous state from providing incorrect results. Execute: `rm -r tests/test-results`
3. Execute:

```
dotnet test RosettaProxy.sln `
    --results-directory "$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results")" `
    --logger trx `
    /p:CollectCoverage=true `
    /p:Include="[LexisNexis.Rosetta.Proxy]*%2c[LexisNexis.Rosetta.Core]*" `
    /p:CoverletOutput="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/")" `
    /p:MergeWith="$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.json")" `
    /p:CoverletOutputFormat="json%2copencover" `
    -m:1
```

### Code coverage with report

To get a report on the code coverage locally from the terminal you will need to install the [dotnet-reportgenerator-globaltool](https://www.nuget.org/packages/dotnet-reportgenerator-globaltool/). To do so run `dotnet tool install --global dotnet-reportgenerator-globaltool`.

1. Execute the steps above to [get code coverage](#code-coverage) since the report will be based on the code coverage files.
2. Browse to the repo's root directory.
3. Execute:

```
reportgenerator `
    "-reports:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/coverage.opencover.xml")" `
    "-targetdir:$(Join-Path -Path (Get-Location) -ChildPath "tests/test-results/coverage-results/report")" `
    -reportTypes:htmlInline
```

4. Open the `index.html` file that gets produced at `/tests/test-results/coverage-results/report`.


  
* overwrite configuration items on options without adding a whole new appsettings
* http mocking (.UseHttpMocks or MockHttpResponse methods) should allow access to the service provider
* investigate serilog logging test extensions for integration tests
* explain how to set loglevels for integration tests output https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0#configure-logging
  * maybe add it as an extension and use it on where I have similar functionality (HostBuilderExtensions and HttpMockServerBuilderExtensions)
* explain workaround for nuget debugging with SourceLink when pdb is part of nuget package
* create an issue on the repo to allow testing https. need to figure out how to configure a cert. Look for TODO on the codebase
* make sure servers are disposed on the tests and demo tests
* publish code coverage to github action artifact
* replace the configuration usages as below with the new extension method to inject configuration values that is also based on the below configuration
```
.ConfigureAppConfiguration((context, builder) =>
                {
                    var memoryConfigurationSource = new MemoryConfigurationSource
                    {
                        InitialData = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("SomeOption", "some value")
                            }
                    };
                    builder.Add(memoryConfigurationSource);
                })
```

## Notes

If you have problems with SSL certificates when running the demos or tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
