# GitHub worflows

There are two workflows setup on this repo:

| Worflow                                                                                       | Status and link                                                                                                                                                                                                                                                               | Description                                                                                       |
| --------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------ |
| [build-test-package](/.github/workflows/build-test-package.yml)                               | [![Build, test and package](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/build-test-package.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/build-test-package.yml)                                          | Builds the solution, runs tests and creates the NuGet packages.                                   |
| [upload-coverage-to-codecov](/.github/workflows/upload-coverage-to-codecov.yml)               | [![Upload coverage to Codecov](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/upload-coverage-to-codecov.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/upload-coverage-to-codecov.yml)                       | Uploads code coverage to Codecov.                                                                 |
| [codeql](/.github/workflows/codeql.yml)                                                       | [![CodeQL](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/codeql.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/codeql.yml)                                                                                   | Analyses code quality with the [CodeQL tool](https://github.com/github/codeql).                   |
| [dispatch-commands](/.github/workflows/dispatch-commands.yml)                                 | [![Slash command dispatch](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dispatch-commands.yml)                                             | Handles slash commands on issues and triggers repository/workflow dispatch events.                |
| [dotnet-format](/.github/workflows/dotnet-format.yml)                                         | [![dotnet format](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format.yml)                                                              | Runs [dotnet format](https://github.com/dotnet/format) and stores the result.                     |
| [dotnet-format-apply-changes](/.github/workflows/dotnet-format-apply-changes.yml)             | [![dotnet format - apply changes](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format-apply-changes.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format-apply-changes.yml)                  | Applies the result from the [dotnet-format workflow](/.github/workflows/dotnet-format.yml).       |
| [markdown-link-check](/.github/workflows/markdown-link-check.yml)                             | [![Markdown link check](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check.yml)                                            | Checks markdown files for broken links and stores the result.                                     |
| [markdown-link-check-handle-result](/.github/workflows/markdown-link-check-handle-result.yml) | [![Markdown link check - broken links](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check-handle-result.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check-handle-result.yml) | Processes the result of the markdown link check workflow.                                         |
| [nuget-release](/.github/workflows/nuget-release.yml)                                         | [![NuGet release](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release.yml)                                                              | Starting workflow to release a NuGet package.                                                     |
| [nuget-publish](/.github/workflows/nuget-publish.yml)                                         | [![NuGet publish](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml)                                                              | Publishes NuGet packages to nuget.org and creates a GitHub release.                               |
| [nuget-release-flow](/.github/workflows/nuget-release-flow.yml)                               | [![NuGet release flow](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release-flow.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release-flow.yml)                                               | Tracks the NuGet release flow by updating the release flow diagram and closing the release issue. |
| [pr-dependabot-auto-merge](/.github/workflows/pr-dependabot-auto-merge.yml)                   | [![PR Dependabot auto merge](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-dependabot-auto-merge.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-dependabot-auto-merge.yml)                             | Automatically merges Dependabot PRs.                                                              |
| [pr-test-results-comment](/.github/workflows/pr-test-results-comment.yml)                     | [![PR test results comment](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-test-results-comment.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-test-results-comment.yml)                                | Adds test results as comments on Pull Requests                                                    |

## Workflows' documentation

- [build-test-package](/docs/dev-notes/workflows/build-test-package-workflow.md)
- [upload-coverage-to-codecov](/docs/dev-notes/workflows/upload-coverage-to-codecov.md)
- [codeql](/docs/dev-notes/workflows/codeql-workflow.md)
- [dispatch-commands](/docs/dev-notes/workflows/dispatch-commands-workflow.md)
- [dotnet-format](/docs/dev-notes/workflows/dotnet-format-workflow.md)
- [dotnet-format-apply-changes](/docs/dev-notes/workflows/dotnet-format-apply-changes-workflow.md)
- [markdown-link-check](/docs/dev-notes/workflows/markdown-link-check-workflow.md)
- [markdown-link-check-handle-result](/docs/dev-notes/workflows/markdown-link-check-handle-result-workflow.md)
- [nuget-release](/docs/dev-notes/workflows/nuget-release-workflow.md)
- [nuget-publish](/docs/dev-notes/workflows/nuget-publish-workflow.md)
- [nuget-release-flow](/docs/dev-notes/workflows/nuget-release-flow-workflow.md)
- [pr-dependabot-auto-merge](/docs/dev-notes/workflows/pr-dependabot-auto-merge-workflow.md)
- [pr-test-results-comment](/docs/dev-notes/workflows/pr-test-results-comment-workflow.md)

## Workflow's security

All the workflows have the minimum required `GITHUB_TOKEN` permissions. Furthermore, the workflows that require priviliged context are separated from the ones that could potentially executed malicious code. The main purpose is to protect from the threat of malicious pull requests. For more information see:

- [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
- [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)

Other relevant links:

- [Permissions for the GITHUB_TOKEN](https://docs.github.com/en/actions/security-guides/automatic-token-authentication#permissions-for-the-github_token)
- [Syntax for GITHUB_TOKEN permissions](https://docs.github.com/en/actions/learn-github-actions/workflow-syntax-for-github-actions#permissions)
- [Breakdown of GITHU_TOKEN permissions by API calls](https://docs.github.com/en/rest/reference/permissions-required-for-github-apps)

## Debugging workflows

You can print [github context objects](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions) by using the [`toJSON` function](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#tojson).

Example with a step running powershell:

```powershell
- name: Dump github context
  shell: pwsh
  run: Write-Output '${{ toJson(github) }}'
```

It's useful to look at the [workflow run logs](https://docs.github.com/en/actions/managing-workflow-runs/using-workflow-run-logs), specially at the `set up job` section which is were you can find for example the permissions assigned to the `GITHUB_TOKEN` that the job will use.

You can also [enable debug logging](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/enabling-debug-logging).
