# dispatch-commands workflow

[![Slash command dispatch](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml)

[This workflow](/.github/workflows/dispatch-commands.yml) handles slash commands on issues and triggers repository/workflow dispatch events. A better description can be found on the [README](https://github.com/peter-evans/slash-command-dispatch) of the github action that enables this workflow.

This workflow only dispatches repository/workflow events, other workflows will handle them.

The commands handled by this workflow are:

- `retry-nuget-release` comment on GitHub issues. This will be handled by the [nuget-release workflow](/docs/dev-notes/workflows/nuget-release-workflow.md). It restarts the [NuGet package release process](/docs/dev-notes/workflows/nuget-release-flow.md). If the issue is not tagged with `nuget-release` then the command will not do anything.

## Secrets

This workflow uses a custom secret `PUBLIC_REPO_SCOPE_GH_TOKEN`. This secret contains a GitHub token with `repo/public_repo` scope and has no expiration date. [Read here](https://github.com/peter-evans/slash-command-dispatch#token) for further details on why this token is required.
