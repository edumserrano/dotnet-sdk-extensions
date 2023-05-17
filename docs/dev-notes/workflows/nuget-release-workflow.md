# nuget-release workflow

[![NuGet release](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release.yml)

[This workflow](/.github/workflows/nuget-release.yml):

- Parses the data created from the [NuGet release Github issue form](/docs/dev-notes/workflows/nuget-release-flow.md).
- Handles retries triggered from the Nuget release issue. Adding a comment on the NuGet release issue with the value `/retry-nuget-release` is handled via the [dispatch-commands workflow](/docs/dev-notes/workflows/dispatch-commands-workflow.md) which will in turn trigger this workflow.
- Uploads a nuget release info artifact which contains information about the NuGet release. This will later be used by the [nuget-publish workflow](/docs/dev-notes/workflows/nuget-publish-workflow.md) to publish the NuGet to [nuget.org](https://www.nuget.org/) and create a GitHub release.
- Checks if there is any open PR for NuGet release. PRs for NuGet release have the `nuget-release` label and a label with the NuGet id.
- If there is an open PR open, the workflow stops.
- If there isn't an open PR, one is created one where the NuGet version in the appropriate `csproj` is updated to the version to be released.
- Adds a status check on the PR that links to this workflow. The status check will later be used by the [nuget-release-flow workflow](/docs/dev-notes/workflows/nuget-release-flow-workflow.md) to access the nuget release info artifact uploaded by this workflow.
- Uploads a nuget release flow info artifact which will be used by the [nuget-release-flow workflow](/docs/dev-notes/workflows/nuget-release-flow-workflow.md) to update the NuGet release flow diagram on the Github issue for the NuGet release.
- Sets the PR to auto-merge.

## Secrets

This workflow uses a custom secret `PUBLIC_REPO_SCOPE_GH_TOKEN`. This secret contains a GitHub token with `repo/public_repo` scope and has no expiration date. Without using a custom GitHub token new workflows that should be triggered by this workflow would not be triggered. For instance, a pull request created by this workflow would not cause the PR checks to rerun. For more information read the [docs](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#using-the-github_token-in-a-workflow):
> When you use the repository's GITHUB_TOKEN to perform tasks on behalf of the GitHub Actions app, events triggered by the GITHUB_TOKEN will not create a new workflow run.
