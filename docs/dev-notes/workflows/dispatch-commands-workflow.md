# dispatch-commands workflow

[![Slash command dispatch](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml)

[This workflow](/.github/workflows/dispatch-commands.ymlyml) handles slash commands on issues and triggers repository/workflow dispatch events. A better description can be found on the [README](https://github.com/peter-evans/slash-command-dispatch) of the github action that enables this workflow.

This workflow only dispatches repository/workflow events, other workflows will handle them.

## Secrets

This workflow uses a custom secret `DISPATCH_COMMANDS_GH_TOKEN`. This secret contains a GitHub token with permissions to push to the repo and has no expiration date. [Read here](https://github.com/peter-evans/slash-command-dispatch#token) for further details on why this token is required.
