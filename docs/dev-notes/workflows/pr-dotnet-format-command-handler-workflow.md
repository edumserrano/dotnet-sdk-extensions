# pr-dotnet-format-command-handler worflow

[![PR dotnet format command handler](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml)

[This workflow](/.github/workflows/pr-dotnet-format-command-handler.yml) is triggered when someone adds a comment on a PR with the following body: `/dotnet-format`.

The workflow will run [dotnet format](https://github.com/dotnet/format) on the PR that was commented and if there are any changes detected it will push those changes to the PR's branch. Finally, it will add a comment on the PR with the results of the `dotnet format` command.

## Secrets

This workflow uses a custom secret `DOTNET_FORMAT_GH_TOKEN`. This secret contains a GitHub token with permissions to push to the repo and has no expiration date. Without using a custom GitHub token, when this workflow pushes the `dotnet format` changes to the branch, the PR checks would not be re-executed because no workflows would be triggered. For more information read the [docs](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#using-the-github_token-in-a-workflow):
> When you use the repository's GITHUB_TOKEN to perform tasks on behalf of the GitHub Actions app, events triggered by the GITHUB_TOKEN will not create a new workflow run.
