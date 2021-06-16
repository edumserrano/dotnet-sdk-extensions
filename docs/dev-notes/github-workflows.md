# GitHub worflows

There are two workflows setup on this repo:

| Worflow                   |      Status and link      |  Description      |
|---------------------------|:-------------------------:|:-----------------:|
| [nuget-publish](/.github/workflows/nuget-publish.yml)             |  [![Publish Nuget packages](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml) | Main workflow to build the code, run tests and publish NuGets |
| [dependabot-auto-merge-pr](/.github/workflows/dependabot-auto-merge-pr.yml)             |  [![Dependabot auto merge PR](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dependabot-auto-merge-pr.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dependabot-auto-merge-pr.yml) | Used to auto merge dependabot PRs |

## Secrets

The [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/main/.github/workflows/nuget-publish.yml) workflow uses one secret for the NuGet API key used to publish the NuGet packages.

This API key will expire on  and will have to be renewed before 5th February 2022 or the workflwo will fail.

## Nuget push action

Initially the nuget package and symbols push step was being done by:

```
dotnet nuget push ./*.nupkg --api-key <api-key> --source https://api.nuget.org/v3/index.json --skip-duplicate
```

This step had a flag set to skip publishing the NuGet (.nupgk) if the version has already been publish. This allowed the workflow to run without failing even if we didn't want to publish a new version of the package.

However there was an issue with this approach in that even if the nuget package already existed the `--skip-duplicate` flag only makes it so that the nuget push command doesn't fail due to the returned 409 from the server but it still tries to push the symbols package after.

The above doesn't fail but it makes NuGet.org send emails to the owner of the package with the following:
 
```
Symbols package publishing failed. The associated symbols package could not be published due to the following reason(s):
The uploaded symbols package contains pdb(s) for a corresponding dll(s) not found in the nuget package.
Once you've fixed the issue with your symbols package, you can re-upload it.

Please note: The last successfully published symbols package is still available for debugging and download.
```

The above error message is also displayed on the nuget's package page even though it's only visible to the owner of the package.

For more information about this see:

- [dotnet nuget push with --skip-duplicate pushes .snupkg constantly and causes validation to fail.](https://github.com/NuGet/Home/issues/10475)
- [When nupkg exists on push --skip-duplicate, don't automatically push snupkg](https://github.com/NuGet/Home/issues/9647)
- [[Symbols] Support removing snupkg validation error messages](https://github.com/NuGet/NuGetGallery/issues/8036)

To avoid this happening I created [a nuget push action](/.github/actions/nuget-push/action.yml) that:

1. Tries to do a push only of the nuget package and only if it succeeds attempts to do a following push of the symbols package.
2. If the nuget push returns a 409 because the package exists that is outputted by the action and the symbols push is skipped.
3. If the nuget push fails because of any other reason the output from nuget push is outputted by the action and the symbols push is skipped.

The action was created following the guidance from [Creating a composite run steps action](https://docs.github.com/en/actions/creating-actions/creating-a-composite-run-steps-action). Even though GitHub's action docs don't mention how to reference an action from a local repo, it's possible by using the full path to the action.

Also note that the filename for the custom action must be `action.yml`.

## Dependabot

### Deleting branches from Dependabot pull requests

The [dependabot-auto-merge-pr](/.github/workflows/dependabot-auto-merge-pr.yml) workflow does not delete the merged branch explicitly because when I tried to use the flag `--delete-branch` from [gh pr merge](https://cli.github.com/manual/gh_pr_merge), GitHub's CLI tries to delete both the remote and local branch. Although it successfully deletes the remote branch, it **fails to delete the local branch** because the workflow does not execute the `gh pr merge` command in a git directory, in fact the workflow does not even checkout the repo.

Although there could have been other ways to deal with this I decided to use a GitHub [repository configuration](/docs/dev-notes/dev-notes-main.md#repository-configuration) that will automatically delete branches once PRs are merged.

### Ignored NuGets

On the [dependabot configuration file](/.github/dependabot.yml) the NuGet `Microsoft.AspNetCore.Mvc.Testing` is ignored because at the moment the `DotNet.Sdk.Extensions.Testing` project where the NuGet is used targets two target frameworks  and has an `if condition` to use different NuGet versions depending on the target framework.

Dependabot does not know that for target framework netcoreapp3.1 the Microsoft.AspNetCore.Mvc.Testing nuget cannot be higher than 3.x.x. This NuGet needs to be manually updated for the other target framework.

### Security considerations when setting up auto merge for dependabot PRs

When a pull request is triggered the `GITHUB_TOKEN` will have [different permissions depending if the pull request came from a forked repo or not](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#how-the-permissions-are-calculated-for-a-workflow-job).

When it comes from a forked repo the `GITHUB_TOKEN` will only have read permissions and won't be able to access any action secrets. This is true at least for public repos, for private repos there are [settings that allow you to control this](https://github.blog/2020-08-03-github-actions-improvements-for-fork-and-pull-request-workflows/).

This plus the fact that a [dependabot pull request is treated as if it was opened from a repository fork](https://github.blog/changelog/2021-02-19-github-actions-workflows-triggered-by-dependabot-prs-will-run-with-read-only-permissions/) means that the action to auto merge dependabot PRs couldn't be done as part of the main `nuget-publish` workflow without potentially introducing security vulnerabilities.

GitHub has all these limitations in place for security reasons. For more information and alternatives read [Keeping your GitHub Actions and workflows secure: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/).

As a result, the `dependabot-auto-merge-pr` workflow, which is responsible for merging a dependabot PR, is a separate workflow that is [triggered from the completion of the main workflow](https://docs.github.com/en/actions/reference/events-that-trigger-workflows#workflow_run). **This workflow runs in a privileged workflow context which means that the `GITHUB_TOKEN` will have write permissions and be able to approve and merge the PR**.

As explained by [Keeping your GitHub Actions and workflows secure: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/), anything that is used on a privileged workflow context must be trusted data. For example, binaries built from an untrusted PR, would be a security vulnerability if executed in the privileged workflow_run workflow context. **Since the `dependabot-auto-merge-pr` workflow does not use any data from the workflow that triggered it, apart from the PR number, there is no security risk**.

#### Extra note

As per [Github Actions and the threat of malicious pull requests](https://nathandavison.com/blog/github-actions-and-the-threat-of-malicious-pull-requests):

> Really, the answer to this is simple - if you're using the pull_request_target event in Github Actions, don't use actions/checkout to then checkout the pull request's code. If you do, then you are opening yourself up to the malicious pull request attack.
>
> If you must combine the two, then make sure you guard your configuration with conditions that only runs steps with access to secrets when the pull request being checked out in the workflow is trusted, whatever that means to you and your requirements. If you search Github, you will find configurations that use the if: feature to do something like this - be careful that your logic is not faulty and test, test, test. Use a non privileged account to fork the repo, and try and exploit it using the techniques covered.

### Fetch Metadata Action

The [dependabot/fetch-metadata](https://github.com/dependabot/fetch-metadata) can be used to extract information about the dependencies being updated by a Dependabot generated PR.

This output from that action could be stored as artifacts if the information is required by a priviliged workflow. One could use the [actions/upload-artifact@v2](https://github.com/actions/upload-artifact) action to upload artifacts from the non provoliged workflow and the [dawidd6/action-download-artifact@v2](https://github.com/dawidd6/action-download-artifact) to download artifacts on the priviliged workflow context. For an example see commit [cleanup workflows](https://github.com/edumserrano/dot-net-sdk-extensions/commit/fffb5dea150f5cbc94fc413f559f47eda2886329) which shows how these were being used in an earlier version of the workflow for auto merge of dependabot PRs.

## Debugging workflows

You can print [github context objects](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions) by using the [`toJSON` function](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#tojson).

Powershell example:

```powershell
$githubContext = '${{toJSON(github)}}'
Write-Host githubContext
```

It is also useful to look at the [workflow run logs](https://docs.github.com/en/actions/managing-workflow-runs/using-workflow-run-logs), specially at the `set up job` section which is were you can find for example the permissions assigned to the `GITHUB_TOKEN` that the job will use.
