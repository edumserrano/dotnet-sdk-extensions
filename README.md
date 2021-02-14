# dot-net-sdk-extensions

This repo contains extensions to use with .NET applications as weel as extensions for unit and [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

It also contains guides on scenarios around building apps using .NET SDK. These guides are for situations where an extension is not merited but some documentation on how to accomplish the task is.

## Extensions list

The extensions available are split into two groups:

* Extensions to use on app code.
* Extensions to use when doing integration and unit tests.

For more information about each extension check its docs and demo. You can find the link to each extension's documentation below.

### For apps

* [Eagerly validating options](/docs/configuration/options-eagerly-validation.md)
* [Using `T` options classes instead of `IOptions<T>`](/docs/configuration/options-without-IOptions.md)

### For integration tests

* [Providing test appsettings files to the test server](/docs/integration-tests/configuring-webhost.md)
* [Mocking HttpClient's responses in-process](/docs/integration-tests/http-mocking-in-process.md)
* [Mocking HttpClient's responses out-of-process](/docs/integration-tests/http-mocking-out-of-process.md)
* [Integration tests for HostedServices (Background Services)](/docs/integration-tests/hosted-services.md)

### For unit tests

* [Mocking HttpClient's responses for unit testing](/docs/unit-tests/http-mocking-unit-tests.md)

### Other

* [Notes on WebApplicationFactory regarding asp.net integration tests](/docs/integration-tests/web-application-factory.md)

## Guides

* [Use cases for generic host](/docs/guides/generic-host-use-cases.md)

## TODO

* split demo projects, src for extensions and guides (create a demo project to explain multiple startup classes in same project) and add readme to each
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
* get lambda extensions 
* investigate serilog logging test extensions for integration tests
* Create ci cd pipeline, add badge to readme (link to pipeline, code coverage)
* Publish nuget packages
* when I have the nuget create a separate sln for the demos using the nuget instead of project reference
* Setup hooks to run pipelines on pull requests
* explain how to set loglevels for integration tests output https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0#configure-logging
  * maybe add it as an extension and use it on where I have similar functionality (HostBuilderExtensions and HttpMockServerBuilderExtensions)
* explain workaround for nuget debugging with SourceLink when pdb is part of nuget package
* create an issue on the repo to allow testing https. need to figure out how to configure a cert. Look for TODO on the codebase
* make sure servers are disposed on the tests and demo tests

## Notes

If you have problems with SSL certificates when running the demos or tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
