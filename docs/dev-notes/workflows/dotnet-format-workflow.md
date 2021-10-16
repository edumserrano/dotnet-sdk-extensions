# dotnet-format workflow

[![dotnet format](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml)

[This workflow](/.github/workflows/dotnet-format.yml) runs [dotnet format](https://github.com/dotnet/format) on pushes to the main branch and creates a pull request when changes are required. The PR is created with a label `dotnet-format`. To avoid creating multiple pull requests about formatting issues, this workflow will only create a pull request if there is no open pull request with the `dotnet-format` label.

The dotnet format will report violations based on the [.editorconfig](/.editorconfig) file and the analyzers included in each project. Note that in addition to the analyzers each `csproj` has, the [Directory.Build.props](/docs/dev-notes/dev-notes-main.md#projects-wide-configuration) files also add several analyzers added to the projects.

## Secrets

This workflow uses a custom secret `DOTNET_FORMAT_GH_TOKEN`. This secret contains a GitHub token with permissions to push to the repo and has no expiration date. Without using a custom GitHub token the PR would be created but no checks would be executed because no workflows would be triggered. For more information read the [docs](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#using-the-github_token-in-a-workflow):
> When you use the repository's GITHUB_TOKEN to perform tasks on behalf of the GitHub Actions app, events triggered by the GITHUB_TOKEN will not create a new workflow run.
