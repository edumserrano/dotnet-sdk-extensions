# dotnet-format workflow

[![dotnet format](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml)

[This workflow](/.github/workflows/dotnet-format.yml) runs [dotnet format](https://github.com/dotnet/format) on pushes to the main branch and on pull requests. It also uploads two artifacts:

- an artifact containing all the files changed by `dotnet format`. The folder structure for the changed files is kept on this artifact.
- a workflow info artifact that contains a summary `json` file which indicates if the `dotnet format` produced any changed files as well as some other required info that will be used by the [dotnet-format-apply-changes workflow](/docs/dev-notes/workflows/dotnet-format-apply-changes-workflow.md) to apply the changed files.

The `dotnet format` will report violations based on the [.editorconfig](/.editorconfig) file and the analyzers included in each project. Note that in addition to the analyzers each `csproj` has, the [Directory.Build.props](/docs/dev-notes/README.md#projects-wide-configuration) file also adds several analyzers to the projects.

> **Note**
>
> The reason to split this workflow in two, one that runs `dotnet format` and one that applies the results of the `dotnet format` workflow is security. Workflows that require priviliged context are separated from the ones that could potentially executed malicious code. The main purpose is to protect from the threat of malicious pull requests. For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
