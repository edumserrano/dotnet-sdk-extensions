# GitHub worflows

There are two workflows setup on this repo:

| Worflow                                                                     |                                                                                                                   Status and link                                                                                                                   |                                                            Description                                                             |
| --------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :-------------------------------------------------------------------------------------------------------------------------------:  |
| [nuget-publish](/.github/workflows/nuget-publish.yml)                       |             [![Publish Nuget packages](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml)             |                                   Main workflow to build the code, run tests and publish NuGets.                                   |
| [codeql](/.github/workflows/codeql.yml)                                     |                            [![CodeQL](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml)                            |                          Analyses code quality with the [CodeQL tool](https://github.com/github/codeql).                           |
| [dotnet-format](/.github/workflows/dotnet-format.yml)                       |                 [![dotnet format](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml)                  |        Runs [dotnet format](https://github.com/dotnet/format) on the main branch and creates PRs when changes are required.        |
| [dispatch-commands](/.github/workflows/dispatch-commands.yml)               |                 [![Slash command dispatch](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dispatch-commands.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dispatch-commands.yml) |        Handles slash commands on issues and triggers repository/workflow dispatch events.                                          |
| [pr-dependabot-auto-merge](/.github/workflows/pr-dependabot-auto-merge.yml) | [![PR Dependabot auto merge](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dependabot-auto-merge.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dependabot-auto-merge.yml) |                                                Automatically merges Dependabot PRs.                                                |
| [pr-dotnet-format-check](/.github/workflows/pr-dotnet-format-check.yml)     | [![PR dotnet format check](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml)       | Runs [dotnet format](https://github.com/dotnet/format) on pull requests and creates a comment on the PR when changes are required. |
| [pr-dotnet-format-command-handler](/.github/workflows/pr-dotnet-format-command-handler.yml) | [![PR dotnet format command handler](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml)       | Handles `/dotnet-format` comments on pull requests and updates them with the results from running [dotnet format](https://github.com/dotnet/format). |

## Workflows' documentation

- [codeql](/docs/dev-notes/workflows/codeql-workflow.md)
- [dotnet-format](/docs/dev-notes/workflows/dotnet-format-workflow.md)
- [nuget-publish](/docs/dev-notes/workflows/nuget-publish-workflow.md)
- [pr-dependabot-auto-merge](/docs/dev-notes/workflows/pr-dependabot-auto-merge-workflow.md)
- [pr-dotnet-format-check](/docs/dev-notes/workflows/pr-dotnet-format-check-workflow.md)
- [pr-dotnet-format-command-handler](/docs/dev-notes/workflows/pr-dotnet-format-command-handler-workflow.md)

## Debugging workflows

You can print [github context objects](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions) by using the [`toJSON` function](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#tojson).

Example with a step running powershell:

```powershell
- name: Dump github context
  shell: pwsh
  run: Write-Host '${{ toJson(github) }}'
```

It's useful to look at the [workflow run logs](https://docs.github.com/en/actions/managing-workflow-runs/using-workflow-run-logs), specially at the `set up job` section which is were you can find for example the permissions assigned to the `GITHUB_TOKEN` that the job will use.

You can also [enable debug logging](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/enabling-debug-logging).
