# Dev notes

- [Building](#building)
  - [Building with Visual Studio](#building-with-visual-studio)
  - [Building with dotnet CLI](#building-with-dotnet-cli)
- [Running tests](#running-tests)
  - [Run tests with Visual Studio](#run-tests-with-visual-studio)
  - [Run tests with dotnet CLI](#run-tests-with-dotnet-cli)
- [Projects wide configuration](#projects-wide-configuration)
- [Deterministic Build configuration](#deterministic-build-configuration)
- [Source Link configuration](#source-link-configuration)
- [Repository configuration](#repository-configuration)
- [GitHub Workflows](#github-workflows)

## Building

### Building with Visual Studio

1) Clone the repo and open the **DotNet.Sdk.Extensions.sln** solution file at the root.
2) Press build on Visual Studio.

### Building with dotnet CLI

1) Clone the repo and browse to the root directory of the repo using your favorite shell.
2) Run **`dotnet build DotNet.Sdk.Extensions.sln`** to build the source for the extensions.

## Running tests

The test projects run against multiple frameworks and the [workflow to build and test](/.github/workflows/nuget-publish.yml) the solution runs both on Linux and on Windows.

> **Note**
>
> Some tests run a test server with an HTTPS URL so you have to run the following command to trust developer certificates:
>
> ```
> dotnet dev-certs https --trust
> ```
>
> For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).

### Run tests with Visual Studio

1) Clone the repo and open the **DotNet.Sdk.Extensions.sln** solution file at the root.
2) Go to the test explorer in Visual Studio and run tests.

**Note:** [Remote testing](https://docs.microsoft.com/en-us/visualstudio/test/remote-testing?view=vs-2022) with WSL is configured on the solution which enables you to run the tests locally on Linux via WSL or Docker. You can view the configuration file at [testenvironments.json](/testenvironments.json).

To run tests via WSL you need to have at least `Visual Studio 2022` and the Linux distro `Ubuntu-20.04` installed on [WSL](https://docs.microsoft.com/en-us/windows/wsl/install).

### Run tests with dotnet CLI

1) Clone the repo and browse to the root directory of the repo using your favorite shell.
2) Run **`dotnet test DotNet.Sdk.Extensions.sln`** to runb tests.

## Projects wide configuration

- The [Directory.Build.props](/Directory.Build.props) enables several settings as well as adds some common NuGet packages for all projects.

- There is a set of NuGet packages that are only applied in test projects by using the condition `"'$(IsTestProject)' == 'true'"`. To make this work the `csproj` for the test projects must have the `<IsTestProject>true</IsTestProject>` property defined. Adding this property manually shouldn't be needed because it should be added by the `Microsoft.NET.Test.Sdk` package however there seems to be an issue with this when running tests outside of Visual Studio. See [this GitHub issue](https://github.com/dotnet/sdk/issues/3790#issuecomment-1100773198) for more info.

- When running `dotnet` CLI commands make sure you are at the root of the repo so that the `global.json` is respected. If you don't you might get unexpected results when building the solution. As explained in [global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json):

> The .NET SDK looks for a global.json file in the current working directory (which isn't necessarily the same as the project directory) or one of its parent directories.

## Deterministic Build configuration

Following the guide from [Deterministic Builds](https://github.com/clairernovotny/DeterministicBuilds) the `ContinuousIntegrationBuild` setting on the [Directory.Build.props](/Directory.Build.props) is set to true, if the build is being executed in GitHub actions.

## Source Link configuration

Following the guide from [Source Link](https://github.com/dotnet/sourcelink) the following settings are configured to enable creating Nuget libraries that provide source debugging:

- `PublishRepositoryUrl`
- `EmbedUntrackedSources`
- `IncludeSymbols`
- `SymbolPackageFormat`

These values are configured on the projects that produce NuGet packages:

- [DotNet.Sdk.Extensions.csproj](/src/DotNet.Sdk.Extensions/DotNet.Sdk.Extensions.csproj)
- [DotNet.Sdk.Extensions.Testing.csproj](/src/DotNet.Sdk.Extensions.Testing/DotNet.Sdk.Extensions.Testing.csproj)

In addition, as per the documentation, both the above projects include the `Microsoft.SourceLink.GitHub` NuGet.

## Repository configuration

From all the GitHub repository settings the configurations worth mentioning are:

- **Automatically delete head branches** is enabled: after pull requests are merged, head branches are deleted automatically.
- **Branch protection rules**. There is a branch protection rule for the the `main` branch that will enforce the following:
  - Require status checks to pass before merging.
  - Require branches to be up to date before merging.
  - Require linear history.

## GitHub Workflows

For more information about the GitHub workflows configured for this repo go [here](/docs/dev-notes/workflows/README.md).
