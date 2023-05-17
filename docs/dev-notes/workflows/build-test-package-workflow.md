# build-test-package workflow

[![Build, test and package](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/build-test-package.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/build-test-package.yml)

[This workflow](/.github/workflows/build-test-package.yml):

- Builds the code and runs tests on windows-latest and ubuntu-latest.
- Uploads an opencover code coverage file as a workflow artifact.
- Generates code coverage report and uploads it as a workflow artifact.
- Uploades test results as a workflow artifact.
- Generates NuGet packages and uploads them as a workflow artifact.

## Build warnings will make the workflow fail

The `dotnet build` command includes the `-warnaserror` flag which will cause the build to fail if there are any errors.

This is used to help keep the code healthy whilst balancing local dev. Meaning, when developing locally there is no need to force all warnings to be fixed to be able to build the code.

## Testing loggers

When running tests we use 3 loggers:

- `trx`: normal logger, produces test result files which can be downloaded and viewed on Visual Studio.
- `GitHubActions`: used to produce annotations on the workflow to give more visibility when tests fail. For more info see [GitHub Actions Test Logger](https://github.com/Tyrrrz/GitHubActionsTestLogger). It also adds annotations on PRs.
- `liquid.custom`: Uses a [template](/tests/liquid-test-logger-template.md) to create a markdown reports for the test results. These markdown reports are uploaded as workflow artifacts and in case of Pull Requests they are added as comments. For more info see [Liquid Test Reports](https://github.com/kurtmkurtm/LiquidTestReports).
