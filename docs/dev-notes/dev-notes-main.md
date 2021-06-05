# Dev notes

## GitHub Workflows

| Worflow                   |      Status and link      |
|---------------------------|:-------------------------:|
| [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/main/.github/workflows/nuget-publish.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Publish%20Nuget%20packages/badge.svg) |

For more information about the GitHub actions configured for this repo go [here](/docs/dev-notes/github-workflows.md).

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

## Nullable reference types

Because of the [Directory.Build.props](/Directory.Build.props) that exists at the root of the repo the [nullable referece types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-migration-strategies) are enabled for all projects in the repo.

## Other notes

If you have problems with SSL certificates when running the tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).

