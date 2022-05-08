<!-- nuget-release-flow -->
## NuGet release flow progress

[![Generic badge](https://img.shields.io/badge/status-{{ .nugetReleaseStatus }}-{{ .nugetReleaseBadgeColor }}.svg)](https://shields.io/)

``````mermaid
  graph TD
      A([Create NuGet release issue]):::ok --> B
      B[Action: Issue for NuGet release]:::{{ .issueNugetReleaseNodeStatus }} --> C
      C[Action: NuGet release command handler]:::{{ .nugetReleaseCommandHandlerNodeStatus }} --> D
      D[Pull Request: Release NuGet {{ .nugetId }} {{ .nugetVersion }}]:::{{ .nugetReleasePullRequestNodeStatus }} --> E
      E[Action: Publish NuGet packages]:::{{ .publishNugetNodeStatus }} --> F([NuGet released]):::{{ .publishNugetNodeStatus }}
  classDef ok stroke:#a5e16e 
  classDef error stroke:#ff1355
  classDef default stroke:#99a095
  click B "{{ .issueNugetReleaseUrl }}" _blank
  click C "{{ .nugetReleaseCommandHandlerUrl }}" _blank
  click D "{{ .nugetReleasePullRequestUrl }}" _blank
  click E "{{ .publishNugetUrl }}" _blank
``````

- This shows the overall progress of the NuGet release flow. This is updated in real time, even if you retry the release.
- If any step fails it will be highlighted in red. Click on it to find out more details in the logs of the action.
- To retry add a comment in this issue with `/retry-nuget-release`, this will restart the NuGet release flow. Before retrying make sure to close any pull requests that might have been created by this release flow.

<!-- nuget-id: {{ .nugetId }} -->
<!-- nuget-version: {{ .nugetVersion}} -->
<!-- issue-nuget-release-node-status: {{ .issueNugetReleaseNodeStatus }} -->
<!-- issue-nuget-release-url: {{ .issueNugetReleaseUrl }} -->
<!-- nuget-release-command-handler-node-status: {{ .nugetReleaseCommandHandlerNodeStatus }} -->
<!-- nuget-release-command-handler-url: {{ .nugetReleaseCommandHandlerUrl }} -->
<!-- nuget-release-pull-request-node-status: {{ .nugetReleasePullRequestNodeStatus }} -->
<!-- nuget-release-pull-request-url: {{ .nugetReleasePullRequestUrl }} -->
