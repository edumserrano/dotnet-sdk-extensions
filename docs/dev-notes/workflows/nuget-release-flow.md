# NuGet release flow

To release a NuGet package you only need to open an issue using the `Release NuGet package` template. Once the NuGet is released the issue is updated and closed.

The NuGet release flow is as follows:

1) Create an issue using the `Release NuGet package` template.
2) Opening the issue will trigger the [nuget-release workflow](/docs/dev-notes/workflows/nuget-release-workflow.md). This workflow will create a PR which updates the NuGet package version to be released.
3) The PR is set to auto-complete so once all the checks pass the PR is merged.
4) When the PR completes the [nuget-publish workflow](/docs/dev-notes/workflows/nuget-publish-workflow.md) is triggered which publishes the NuGet package to nuget.org and creates a GitHub release.
5) The NuGet release issue is closed.

During this flow the [nuget-release-flow workflow](/docs/dev-notes/workflows/nuget-release-flow-workflow.md) will update the NuGet release issue with a diagram that shows the status for each of the release steps.

If something fails during the release flow, the NuGet release issue diagram is updated accordingly and you can choose to restart the process by adding the following comment to the issue:
> /retry-nuget-release

## Security considerations

Only users with write access to the repo will be release a NuGet package version. If a user without write access:

- creates a `Release NuGet package` issue then the issue will be automaticallyu closed without doing the release.
- tries to restart a NuGet package release by using the `/retry-nuget-release` command then the [workflow processing the command](/docs/dev-notes//workflows/dispatch-commands-workflow.md) will ignore it.
