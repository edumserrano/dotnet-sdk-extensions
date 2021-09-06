# GitHub worflows

There are two workflows setup on this repo:

| Worflow                                                                     |                                                                                                                   Status and link                                                                                                                   |                                                            Description                                                            |
| --------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :-------------------------------------------------------------------------------------------------------------------------------: |
| [nuget-publish](/.github/workflows/nuget-publish.yml)                       |             [![Publish Nuget packages](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml)             |                                   Main workflow to build the code, run tests and publish NuGets                                   |
| [dependabot-auto-merge-pr](/.github/workflows/dependabot-auto-merge-pr.yml) | [![Dependabot auto merge PR](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dependabot-auto-merge-pr.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dependabot-auto-merge-pr.yml) |                                                Automatically merges Dependabot PRs                                                |
| [codeql](/.github/workflows/codeql.yml)                                     |                            [![CodeQL](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/codeql.yml)                            |                          Analyses code quality with the [CodeQL tool](https://github.com/github/codeql)                           |
| [dotnet-format](/.github/workflows/dotnet-format.yml)                       |                 [![dotnet format](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format.yml)                  |        Runs [dotnet format](https://github.com/dotnet/format) on the main branch and creates PRs when changes are required        |
| [dotnet-format-pr](/.github/workflows/dotnet-format-pr.yml)                 |           [![dotnet format check](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format-pr.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dotnet-format-pr.yml)            | Runs [dotnet format](https://github.com/dotnet/format) on pull requests and creates a comment on the PR when changes are required |

## Debugging workflows

You can print [github context objects](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions) by using the [`toJSON` function](https://docs.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#tojson).

Powershell example:

```powershell
$githubContext = '${{toJSON(github)}}'
Write-Host $githubContext
```

It's useful to look at the [workflow run logs](https://docs.github.com/en/actions/managing-workflow-runs/using-workflow-run-logs), specially at the `set up job` section which is were you can find for example the permissions assigned to the `GITHUB_TOKEN` that the job will use.

You can also [enable debug logging](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/enabling-debug-logging).

## nuget-publish workflow

This workflow will:

1) Always builds the code and runs tests on windows-latest and ubuntu-latest.
2) On ubuntu-latest, generates code coverage and uploads it to Codecov and as a workflow artifact.
3) On ubuntu-latest, generates NuGet packages and uploads them as a workflow artifact.
4) Publishes NuGet packages if the workflow was not triggered by a pull request.

### Secrets

The [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/main/.github/workflows/nuget-publish.yml) workflow uses one secret for the NuGet API key used to publish the NuGet packages.

This API key will expire on  and will have to be renewed before 5th February 2022 or the workflow will fail.

### Codecov

Codecov integration does not require any secret, it was done via the [Codecov GitHub app](https://github.com/apps/codecov).

Besides the information available on the [Codecov web app](https://app.codecov.io/gh/edumserrano/dot-net-sdk-extensions), this integration enables Codecov to:

- [add status checks on pull requests](https://docs.codecov.com/docs/commit-status)
- [display coverage on pull requests via comments](https://docs.codecov.com/docs/pull-request-comments)
- [add line-by-line coverage on pull requests via file annotations](https://docs.codecov.com/docs/github-checks)

The [Codecov configuration file](/.github/codecov.yml) contains additional configuration for Codecov.

### NuGet push action

Initially the NuGet package and symbols push step was being done by:

```
dotnet nuget push ./*.nupkg --api-key <api-key> --source https://api.nuget.org/v3/index.json --skip-duplicate
```

This step had a flag set to skip publishing the NuGet (.nupgk) if the version has already been publish. This allowed the workflow to run without failing even if we didn't want to publish a new version of the package.

However there was an issue with this approach in that even if the NuGet package already existed the `--skip-duplicate` flag only makes it so that the `nuget push` command doesn't fail due to the returned 409 from the server but it still tries to push the symbols package after.

The above doesn't fail but it makes nuget.org send emails to the owner of the package with the following:

```
Symbols package publishing failed. The associated symbols package could not be published due to the following reason(s):
The uploaded symbols package contains pdb(s) for a corresponding dll(s) not found in the NuGet package.
Once you've fixed the issue with your symbols package, you can re-upload it.

Please note: The last successfully published symbols package is still available for debugging and download.
```

The above error message is also displayed on the NuGet's package page even though it's only visible to the owner of the package.

For more information about this see:

- [dotnet nuget push with --skip-duplicate pushes .snupkg constantly and causes validation to fail.](https://github.com/NuGet/Home/issues/10475)
- [When nupkg exists on push --skip-duplicate, don't automatically push snupkg](https://github.com/NuGet/Home/issues/9647)
- [[Symbols] Support removing snupkg validation error messages](https://github.com/NuGet/NuGetGallery/issues/8036)

To avoid this happening I created [a nuget push action](/.github/actions/nuget-push/action.yml) that:

1. Tries to do a push only of the `nuget package` and only if it succeeds attempts to do a following push of the symbols package.
2. If the `nuget push` returns a 409 because the package exists that is outputted by the action and the symbols push is skipped.
3. If the `nuget push` fails because of any other reason the output from `nuget push` is shown by the action and the symbols push is skipped.

The action was created following the guidance from [Creating a composite run steps action](https://docs.github.com/en/actions/creating-actions/creating-a-composite-run-steps-action). Even though GitHub's action docs don't mention how to reference an action from a local repo, it's possible by using the full path to the action.

Also note that the filename for the custom action must be `action.yml`.

## dependabot-auto-merge-pr workflow

The purpose of this workflow is to auto merge pull requests generated by Dependabot.

This workflow is triggered once the `nuget-publish` workflow completes but will only execute if it was triggered by the `dependabot[bot]` actor. The flow is as follows:

1) Pull request generated.
2) `nuget-publish` workflow is triggered.
3) Once `nuget-publish` workflow completes, the `dependabot-auto-merge-pr` workflow is triggered.
4) If the actor **is not** `dependabot[bot]` then the workflow terminates. Else, it approves and merges the pull request.

Note that when the `dependabot-auto-merge-pr` is triggered, its actor will be the actor that triggered the `nuget-publish` workflow. So if a pull request is triggered by some other actor the `dependabot-auto-merge-pr` will not execute.

With this configuration the `dependabot-auto-merge-pr` workflow will only auto merge pull requests once the build has completed successfully and tests have passed. This is guaranteed by the `nuget-publish` workflow.

The [Dependabot configuration file](/.github/dependabot.yml) contains additional configuration for Dependabot.

### Conditions for auto-merge

Note that for pull requests to automatically be merged the conditions described in [Enable Auto-Merge for PR in Github Action](https://github.com/cli/cli/discussions/3660) must be met.

### Deleting branches from Dependabot pull requests

To be able to delete branches using the flag `--delete-branch` from [gh pr merge](https://cli.github.com/manual/gh_pr_merge), a checkout of the repo is done before or else the command fails because GitHub's CLI tries to delete both the remote and local branch.

**Without** the checkout step the `gh pr merge` command successfully deletes the remote branch, but it **fails to delete the local branch** and the workflow fails.

### Ignored NuGets

On the [Dependabot configuration file](/.github/dependabot.yml) the NuGet `Microsoft.AspNetCore.Mvc.Testing` is ignored because at the moment the `DotNet.Sdk.Extensions.Testing` project where the NuGet is used targets two target frameworks  and has an `if condition` to use different NuGet versions depending on the target framework.

Dependabot does not know that for target framework netcoreapp3.1 the Microsoft.AspNetCore.Mvc.Testing NuGet cannot be higher than 3.x.x. This NuGet needs to be manually updated for the other target framework.

### Security considerations when setting up auto merge for Dependabot PRs

When a pull request is triggered the `GITHUB_TOKEN` will have [different permissions depending if the pull request came from a forked repo or not](https://docs.github.com/en/actions/reference/authentication-in-a-workflow#how-the-permissions-are-calculated-for-a-workflow-job).

As per the [docs](https://docs.github.com/en/code-security/supply-chain-security/keeping-your-dependencies-updated-automatically/automating-dependabot-with-github-actions#responding-to-events):

> Dependabot is able to trigger GitHub Actions workflows on its pull requests and comments; however, due to ["GitHub Actions: Workflows triggered by Dependabot PRs will run with read-only permissions"](https://github.blog/changelog/2021-02-19-github-actions-workflows-triggered-by-dependabot-prs-will-run-with-read-only-permissions/), certain events are treated differently.
>
> For workflows initiated by Dependabot (github.actor == "dependabot[bot]") using the pull_request, pull_request_review, pull_request_review_comment, and push events, the following restrictions apply:
>
> - GITHUB_TOKEN has read-only permissions.
> - Secrets are inaccessible.
>
> For more information, see ["Keeping your GitHub Actions and workflows secure: Preventing pwn requests"](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/).

Even though pull requests on **public repos** from a forked repo will only have a `GITHUB_TOKEN` with read permissions and won't be able to access any action secrets, for **private repos** there are [settings that allow you to control this](https://github.blog/2020-08-03-github-actions-improvements-for-fork-and-pull-request-workflows/).

Given the above restrictions, the action to auto merge Dependabot PRs couldn't be done as part of the main `nuget-publish` workflow without potentially introducing security vulnerabilities. 

As a result, the `dependabot-auto-merge-pr` workflow, which is responsible for merging a Dependabot PR, is a separate workflow that is [triggered from the completion of the main workflow](https://docs.github.com/en/actions/reference/events-that-trigger-workflows#workflow_run). **This workflow runs in a privileged workflow context which means that the `GITHUB_TOKEN` will have write permissions and be able to approve and merge the PR**.

As explained by [Keeping your GitHub Actions and workflows secure: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/), anything that is used on a privileged workflow context must be trusted data. For example, binaries built from an untrusted PR, would be a security vulnerability if executed in the privileged workflow_run workflow context. **Since the `dependabot-auto-merge-pr` workflow does not use any data from the workflow that triggered it, apart from the PR number, there is no security risk**.

#### Extra note

As per [Github Actions and the threat of malicious pull requests](https://nathandavison.com/blog/github-actions-and-the-threat-of-malicious-pull-requests):

> Really, the answer to this is simple - if you're using the pull_request_target event in Github Actions, don't use actions/checkout to then checkout the pull request's code. If you do, then you are opening yourself up to the malicious pull request attack.
>
> If you must combine the two, then make sure you guard your configuration with conditions that only runs steps with access to secrets when the pull request being checked out in the workflow is trusted, whatever that means to you and your requirements. If you search Github, you will find configurations that use the if: feature to do something like this - be careful that your logic is not faulty and test, test, test. Use a non privileged account to fork the repo, and try and exploit it using the techniques covered.

#### Example of handling pull_request events

See [dependabot docs](https://docs.github.com/en/code-security/supply-chain-security/keeping-your-dependencies-updated-automatically/automating-dependabot-with-github-actions#handling-pull_request-events) for code example of how to implement these kind of workflows securily.

#### GitHub blog post series on "Keeping your GitHub Actions and workflows secure"

- [Keeping your GitHub Actions and workflows secure Part 1: Preventing pwn requests](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/)
- [Keeping your GitHub Actions and workflows secure Part 2: Untrusted input](https://securitylab.github.com/research/github-actions-untrusted-input/)
- [Keeping your GitHub Actions and workflows secure Part 3: How to trust your building blocks](https://securitylab.github.com/research/github-actions-building-blocks/)

### Fetch Metadata Action

The [dependabot/fetch-metadata](https://github.com/dependabot/fetch-metadata) can be used to extract information about the dependencies being updated by a Dependabot generated PR.

This output from that action could be stored as artifacts if the information is required by a priviliged workflow. One could use the [actions/upload-artifact@v2](https://github.com/actions/upload-artifact) action to upload artifacts from the non provoliged workflow and the [dawidd6/action-download-artifact@v2](https://github.com/dawidd6/action-download-artifact) to download artifacts on the priviliged workflow context. For an example see commit [cleanup workflows](https://github.com/edumserrano/dot-net-sdk-extensions/commit/fffb5dea150f5cbc94fc413f559f47eda2886329) which shows how these were being used in an earlier version of the workflow for auto merge of Dependabot PRs.

## codeql workflow

This workflow performs [code scanning with CodeQL](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/about-code-scanning). The results are uploaded to the repo and visible on [code scanning alerts](https://github.com/edumserrano/dot-net-sdk-extensions/security/code-scanning). The resulting [`SARIF`](https://sarifweb.azurewebsites.net/) file is also uploaded as a workflow artifact.

When doing pull requests the alerts detected will be visible on via [file annotations](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/triaging-code-scanning-alerts-in-pull-requests).

This workflow produces status checks on pull requests and the repo is configured so that the status check fails if [any alert](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/configuring-code-scanning#defining-the-alert-severities-causing-pull-request-check-failure) is found.

### on:push paths-ignore

[As per documentation](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/configuring-code-scanning#avoiding-unnecessary-scans-of-pull-requests):
>For CodeQL code scanning workflow files, don't use the paths-ignore or paths keywords with the on:push event as this is likely to cause missing analyses. For accurate results, CodeQL code scanning needs to be able to compare new changes with the analysis of the previous commit.

## dotnet-format workflow

This workflow runs [dotnet format](https://github.com/dotnet/format) on pushes to the main branch and creates a pull request when changes are required.

The dotnet format will report violations based on the [.editorconfig](/.editorconfig) file.

There are several analyzers added to the projects via the [Directory.Build.props](/docs/dev-notes/dev-notes-main.md#projects-wide-configuration) files which enhance the results of the dotnet format tool.

## dotnet-format-pr workflow

Similar to the [dotnet-format workflow](#dotnet-format-workflow) but it runs only on pull requests and adds a comment to the pull request with the results of running the dotnet format tool.
