# upload-coverage-to-codecov workflow

[![Upload coverage to Codecov](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/upload-coverage-to-codecov.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/upload-coverage-to-codecov.yml)

[This workflow](/.github/workflows/upload-coverage-to-codecov.yml) uploads the test coverage results from [build-test-package workflow](/docs/dev-notes/workflows/build-test-package-workflow.md) to Codecov.

## Codecov

Codecov integration is done via the [Codecov GitHub app](https://github.com/apps/codecov). Besides the information available on the [Codecov web app](https://app.codecov.io/gh/edumserrano/dotnet-sdk-extensions), this integration enables Codecov to:

- [add status checks on pull requests](https://docs.codecov.com/docs/commit-status)
- [display coverage on pull requests via comments](https://docs.codecov.com/docs/pull-request-comments)
- [add line-by-line coverage on pull requests via file annotations](https://docs.codecov.com/docs/github-checks)

The [Codecov configuration file](/.github/codecov.yml) contains additional configuration for Codecov.

## Secrets

This workflow uses a custom secret `CODECOV_TOKEN`. This secret contains a [token from Codecov](https://app.codecov.io/gh/edumserrano/dotnet-sdk-extensions/settings) with permissions to upload [code coverage to Codecov](https://app.codecov.io/gh/edumserrano/dotnet-sdk-extensions).

> [!NOTE]
>
> The reason to split this workflow from the [build-test-package workflow](/docs/dev-notes/workflows/build-test-package-workflow.md) is that this workflow requires access to secrets and workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions.
