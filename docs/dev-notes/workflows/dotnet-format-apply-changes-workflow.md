# dotnet-format-apply-changes workflow

[![dotnet format - apply changes](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format-apply-changes.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/dotnet-format-apply-changes.yml)

[This workflow](/.github/workflows/dotnet-format-apply-changes.yml) applies the results from the [dotnet-format workflow](/docs/dev-notes/workflows/dotnet-format-workflow.md). It does different actions depending if the `dotnet format` was executed on the main branch or on a pull request branch.

If the `dotnet format` was executed on the main branch:

- If there aren't any files changed from `dotnet format` then the workflow stops.
- If there is any `dotnet format` pull request already open then the workflow stops. `dotnet format` pull requests are identified by the `dotnet-format` label. The idea is that allowing multiple `dotnet format` pull requests to be open would lead to problems with merge conflicts. If there are files that need to be updated as a result of `dotnet format` then those changes are never lost even if a `dotnet format` pull request is already open. Once the existing `dotnet format` pull request is merged the process restarts and if needed a new `dotnet format` pull request will be created.
- If there isn't any open `dotnet format` pull request then it will create a pull request with the files changed from `dotnet format`. The pull request won't be set to auto-complete because these changes are proposed by analyzers and a human review is warranted.

If the `dotnet format` was executed on a pull request branch:

- If there aren't any files changed from `dotnet format` then the workflow stops.
- If there are files changed then they are pushed to the pull request branch and a comment is added to the pull request indicating that this has happened. This commit is a result of changes proposed by analyzers and a human review is warranted.
- Updates the `dotnet format - apply changes / apply dotnet format (workflow_run)` mandatory status check on pull requests. Guarantees that pull requests are compliant with the result of `dotnet format`.

## Secrets

This workflow uses a custom secret `PUBLIC_REPO_SCOPE_GH_TOKEN`. This secret contains a GitHub token with `repo/public_repo` scope and has no expiration date. Without using a custom GitHub token new workflows that should be triggered by this workflow would not be triggered. For instance, a commit done by this workflow to a PR branch would not cause the PR checks to rerun. For more information read the [docs](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#using-the-github_token-in-a-workflow):
> When you use the repository's GITHUB_TOKEN to perform tasks on behalf of the GitHub Actions app, events triggered by the GITHUB_TOKEN will not create a new workflow run.

## Labels

This workflow requires the following labels to be configured on the repo:

- `dotnet-format`: Pull requests created by the dotnet-format-apply-changes workflow.

> [!NOTE]
>
> The reason to split this workflow in two, one that runs dotnet format `(dotnet-format)` and this one that applies the results `(dotnet-format-apply-changes)` is due to security. On GitHub, workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions. The main purpose for this is to protect from the threat of malicious pull requests.
>
> Without doing this, even if security wasn't an issue, the PRs from forked repos would fail when the `dotnet-format` workflow tried to create a PR or push a commit to a PR. Both of these actions are part of the `dotnet format` flow and are executed by this workflow running on a priviliged context.
>
> For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
