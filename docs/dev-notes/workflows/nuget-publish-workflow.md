# nuget-publish workflow

[![NuGet publish](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-publish.yml)

[This workflow](/.github/workflows/nuget-publish.yml):

- Downloads the  appropriate NuGet package from the [build-test-package](/docs/dev-notes/workflows/build-test-package-workflow.md) workflow and publishes them to [nuget.org](https://www.nuget.org/).
- Creates a GitHub release for the published NuGet package.

This workflow can be manually triggered but usually it gets invoked as part of the [NuGet release process](TODO).

## Secrets

This workflow uses a custom secret `NUGET_PUSH_API_KEY`. The secret contains a NuGet API key that is used to publish the NuGet packages. This API key needs to be renewed once a year. At the moment there is no automated process to renew or even just warn that the API key is about to expire.
