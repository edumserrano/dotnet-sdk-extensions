# pr-test-results-comment worflow

[![PR test results comment](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-test-results-comment.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/pr-test-results-comment.yml)

[This workflow](/.github/workflows/pr-test-results-comment.yml) is triggered when the [build-test-package workflow](/docs/dev-notes/workflows/build-test-package-workflow.md) completes on pull requests. The workflow will download the test results markdown report from the `build-test-package` workflow and publish them as a pull request comment.

## Security considerations

Because this workflow requires a `GITHUB_TOKEN` with some write permissions this workflow has been separated from the [nuget-publish workflow](/docs/dev-notes/workflows/nuget-publish-workflow.md). The main intent is to protect from the threat of malicious pull requests. For more information see [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md).

> [!NOTE]
>
> The reason to split this workflow in two, one that produces the test results `(build-test-package)` and this one which uploads them to the pull request, is due to security. On GitHub, workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions. The main purpose for this is to protect from the threat of malicious pull requests.
>
> Without doing this, even if security wasn't an issue, the PRs from forked repos would fail when the `build-test-package` workflow tried to add/update a comment with the test results on the pull request. However, this workflow runs on a priviliged context and will be able to do update the pull request.
>
> For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
