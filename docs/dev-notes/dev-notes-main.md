# Dev notes

## Building

### Using Visual Studio

1) Clone the repo and open the **DotNet.Sdk.Extensions.sln:** solution file at the root.
2) Press build on Visual Studio.

### Using dotnet CLI

1) Clone the repo and browse to the directory using your favorite shell.
2) Run **`dotnet build DotNet.Sdk.Extensions.sln`** to build the source for the extensions.

## Projects wide configuration

The [Directory.Build.props](/Directory.Build.props) at the root of the repo enables for all projects several settings as well as adds some common NuGet packages.

Furthermore, just for test projects, there is another [Directory.Build.props](/tests/Directory.Build.props) that applies some NuGet packages that should be part of all test projects.

The props file for tests is merged with the root level props file so all test projects obey to the same set of settings defined by the root level props file.

## Repository configuration

From all the GitHub repository settings the configurations worth mentioning are:

- **Automatically delete head branches** is enabled: after pull requests are merged, head branches are deleted automatically.
- **Branch protection rules**. There is a branch protection rule for the the `main` branch that will enforce the following:
  - Require status checks to pass before merging.
  - Require branches to be up to date before merging.
  - Require linear history.

## GitHub Workflows

For more information about the GitHub workflows configured for this repo go [here](/docs/dev-notes/workflows/github-workflows.md).

## Other notes

If you have problems with SSL certificates when running the tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
