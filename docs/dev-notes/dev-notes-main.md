# Dev notes

## Building

### Using Visual Studio

1) Clone the repo and open the **DotNet.Sdk.Extensions.sln:** solution file at the root.
2) Press build on Visual Studio.

### Using dotnet CLI

1) Clone the repo and browse to the directory using your favorite shell.
2) Run **`dotnet build DotNet.Sdk.Extensions.sln`** to build the source for the extensions.

## Debugging

The NuGet packages published include symbols generated with [sourcelink](https://github.com/dotnet/sourcelink).

For more information on how to debug the NuGet packages code from your application see:

- [Exploring .NET Core's SourceLink - Stepping into the Source Code of NuGet packages you don't own](https://www.hanselman.com/blog/exploring-net-cores-sourcelink-stepping-into-the-source-code-of-nuget-packages-you-dont-own)
- [How to Configure Visual Studio to Use SourceLink to Step into NuGet Package Source](https://aaronstannard.com/visual-studio-sourcelink-setup/)
- [Source Link - microsoft docs](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)

## Projects wide configuration

The [Directory.Build.props](/Directory.Build.props) at the root of the repo enables for all projects:

- [Nullable referece types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-migration-strategies) are enabled.
- [Repeatable NuGet package restore using lockfile](https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies) are enabled. For more info read the blog post [Enable repeatable package restores using a lock file](https://devblogs.microsoft.com/nuget/enable-repeatable-package-restores-using-a-lock-file/) and the wiki [Enable repeatable package restore using lock file
](https://github.com/NuGet/Home/wiki/Enable-repeatable-package-restore-using-lock-file)
- [Deterministic builds](https://github.com/clairernovotny/DeterministicBuilds) are enabled.

## Repository configuration

From all the GitHub repository settings the configurations worth mentioning are:

- **Automatically delete head branches** is enabled: after pull requests are merged, head branches are deleted automatically.
- **Branch protection rules**. There is a branch protection rule for the the `main` branch that will enforce the following:
  - Require status checks to pass before merging. The required status checks configured are the ones produced by the [main workflow](/docs/dev-notes/github-workflows.md).
  - Require branches to be up to date before merging;
  - Require signed commits;
  - Require linear history.

## GitHub Workflows

For more information about the GitHub workflows configured for this repo go [here](/docs/dev-notes/github-workflows.md).


## Other notes

If you have problems with SSL certificates when running the tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
