# pr-test-results-comment worflow

[![PR test results comment](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-test-results-comment.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-test-results-comment.yml)

[This workflow](/.github/workflows/pr-pr-test-results-comment.yml) is triggered when the [nuget-publish workflow](/docs/dev-notes/workflows/nuget-publish-workflow.md) completes on Pull Requests.

The workflow will download the test results markdown reports from the nuget-publish workflow and publish them as a Pull Request comment.

## Security considerations

Because this workflow requires a `GITHUB_TOKEN` with some write permissions this workflow has been separated from the [nuget-publish workflow](/docs/dev-notes/workflows/nuget-publish-workflow.md). The main intent is to protect from the threat of malicious pull requests. For more information see [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md).
