# pr-dotnet-format-command-handler worflow

[![PR dotnet format command handler](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-command-handler.yml)

[This workflow](/.github/workflows/pr-dotnet-format-command-handler.yml) is triggered when someone adds a comment on a PR with the following body: `/dotnet format`.

The workflow will run [dotnet format](https://github.com/dotnet/format) on the PR that was commented and if there are any changes detected it will push those changes to the PR's branch. Finally, it will add a comment on the PR with the results of the `dotnet format` command.

## Secrets

This workflow uses a custom secret `DOTNET_FORMAT_GH_TOKEN`. This secret contains a GitHub token with permissions to push to the repo and has no expiration date. Without using a custom GitHub token, when this workflow pushes the `dotnet format` changes to the branch, the PR checks would not be re-executed because no workflows would be triggered. For more information read the [docs](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#using-the-github_token-in-a-workflow):
> When you use the repository's GITHUB_TOKEN to perform tasks on behalf of the GitHub Actions app, events triggered by the GITHUB_TOKEN will not create a new workflow run.

## Workflow concurrency

Unlike other workflows this one does not have a concurrency setting defined. If we define a concurrency then a workflow run that was triggered by the expected comment could get cancelled by any other following comment added to the pull request.

As an alternative we could consider adding the comment body `github.event.comment.body` to the concurrency group id. This would mean that workflows would only get cancelled if they had the same comment body, which would mean that for all comments with the same body all workflows in progress would get cancelled except for the last.

Downsides with the approach having the `github.event.comment.body` as part of the concurrency group id:

- workflow runs might fail to start because of invalid characters used in comments or because of comment size. Functionally this is not a problem
because the workflow would always run for the expected comment. However, the workflow history would show fail runs for this error cases which is
not great.

- the purpose of this workflow is to run `dotnet format` and push changes to the PR. If the workflow is cancelled after the changes are pushed to
the PR but before the workflow finishes then we end up in a potential invalid state. Meaning, if for instance I want to take an action after the
changes are pushed, such as adding a comment to the PR, then the workflow could get cancelled before the comment is made on the PR.
In short, there are parts of the workflow that once started should be finished, the workflow should NOT be allowed to be aborted to avoid getting into
a bad state.

Mainly because of the second downside I decided to **NOT** add a concurrency group based on the comment body.
