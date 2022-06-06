# nuget-publish workflow

[![Publish Nuget packages](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml)

[This workflow](/.github/workflows/nuget-publish.yml):

- Builds the code and runs tests on windows-latest and ubuntu-latest.
- Generates code coverage and uploads as a workflow artifact.
- Uploads code coverage to Codecov.
- Uploades test results as a workflow artifact.
- Generates NuGet packages and uploads them as a workflow artifact.
- If the workflow was NOT triggered by a pull request, publishes NuGet packages to nuget.org.

## Secrets

This workflow uses a custom secret `NUGET_PUSH_API_KEY`. The secret contains a NuGet API key that is used to publish the NuGet packages. This API key will expire on the 5th February 2022 and will have to be renewed before that or the workflow will fail.

## Codecov

Codecov integration does not require any secret, it was done via the [Codecov GitHub app](https://github.com/apps/codecov).

Besides the information available on the [Codecov web app](https://app.codecov.io/gh/edumserrano/dotnet-sdk-extensions), this integration enables Codecov to:

- [add status checks on pull requests](https://docs.codecov.com/docs/commit-status)
- [display coverage on pull requests via comments](https://docs.codecov.com/docs/pull-request-comments)
- [add line-by-line coverage on pull requests via file annotations](https://docs.codecov.com/docs/github-checks)

The [Codecov configuration file](/.github/codecov.yml) contains additional configuration for Codecov.

## Build warnings will make the workflow fail

The `dotnet build` command includes the `-warnaserror` flag which will cause the build to fail if there are any errors.

This is used to help keep the code healthy whilst balancing local dev. Meaning, when developing locally there is no need to force all warnings to be fixed to be able to build the code.

## Testing loggers

When running tests we use 3 loggers:

- `trx`: normal logger, produces test result files which can be downloaded and viewed on Visual Studio.
- `GitHubActions`: used to produce annotations on the workflow to give more visibility when tests fail. For more info see [GitHub Actions Test Logger](https://github.com/Tyrrrz/GitHubActionsTestLogger). It also adds annotations on PRs.
- `liquid.custom`: Uses a [template](/tests/liquid-test-logger-template.md) to create a markdown reports for the test results. These markdown reports are uploaded as workflow artifacts and in case of Pull Requests they are added as comments. For more info see [Liquid Test Reports](https://github.com/kurtmkurtm/LiquidTestReports).

