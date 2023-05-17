# nuget-release-flow workflow

[![NuGet release flow](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release-flow.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/nuget-release-flow.yml)

[This workflow](/.github/workflows/nuget-release-flow.yml) updates the diagram for the NuGet release flow that is part of the NuGet release issue. Since the [NuGet release process]((/docs/dev-notes/workflows/nuget-release-flow.md)) consists of several workflows, the diagram aims to provide an overview of the steps, links to the relevant workflows and a status indication on whether the process failed or not. Without the diagram or something similar, there would be no way to track the progress and state of the NuGet release process except manually checking the worklfows involved.

This workflow is triggered upon the completion of the workflows and pull request involved in the NuGet release process and updates the diagram accordingly.

When the last workflow for the NuGet release completes successfully, this workflow is also responsible for closing the GitHub issue created for the NuGet release.
