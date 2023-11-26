# dotnet-format workflow

[![dotnet format](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml)

[This workflow](/.github/workflows/dotnet-format.yml) runs [dotnet format](https://github.com/dotnet/format) on pushes to the main branch and on pull requests. It also uploads two artifacts:

- an artifact containing all the files changed by `dotnet format`. The folder structure for the changed files is kept on this artifact.
- a workflow info artifact that contains a summary `json` file which indicates if the `dotnet format` produced any changed files as well as some other required info that will be used by the [dotnet-format-apply-changes workflow](/docs/dev-notes/workflows/dotnet-format-apply-changes-workflow.md) to apply the changed files.

The `dotnet format` will report violations based on the [.editorconfig](/.editorconfig) file and the analyzers included in each project. Note that in addition to the analyzers each `csproj` has, the [Directory.Build.props](/docs/dev-notes/README.md#projects-wide-configuration) file also adds several analyzers to the projects.

> [!NOTE]
>
> The reason to split this workflow in two, this one that runs dotnet format `(dotnet-format)` and one that applies the results `(dotnet-format-apply-changes)` is due to security. On GitHub, workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions. The main purpose for this is to protect from the threat of malicious pull requests.
>
> Without doing this, even if security wasn't an issue, the PRs from forked repos would fail when the `dotnet-format` workflow tried to create a PR or push a commit to a PR. Both of these actions are part of the `dotnet format` flow and are executed by the [dotnet-format-apply-changes workflow](/docs/dev-notes/workflows/dotnet-format-apply-changes-workflow.md) running on a priviliged context.
>
> For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
