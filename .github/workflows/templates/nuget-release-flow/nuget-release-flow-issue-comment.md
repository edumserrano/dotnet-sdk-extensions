<!-- nuget-release-flow -->
## NuGet release flow progress

[![Generic badge](https://img.shields.io/badge/status-{{ .nugetReleaseStatus }}-{{ .nugetReleaseBadgeColor }}.svg)](https://shields.io/)

``````mermaid
  graph TD
      A([Create NuGet release issue]):::ok --> B
      B[Action: NuGet release]:::{{ .issueNugetReleaseNodeStatus }} --> C
      C[Pull Request: Release NuGet {{ .nugetId }} {{ .nugetVersion }}]:::{{ .nugetReleasePullRequestNodeStatus }} --> D
      D[Action: NuGet publish]:::{{ .publishNugetNodeStatus }} --> E([NuGet released]):::{{ .publishNugetNodeStatus }}
  classDef ok stroke:#a5e16e
  classDef error stroke:#ff1355
  click B "{{ .issueNugetReleaseUrl }}" _blank
  click C "{{ .nugetReleasePullRequestUrl }}" _blank
  click D "{{ .publishNugetUrl }}" _blank
``````

- This shows the overall progress of the NuGet release flow. It might take some time for the diagram state to update, please be patient.
- If any step fails it will be highlighted in red. Click on it to find out more details in the logs of the action.
- To retry add a comment in this issue with `/retry-nuget-release`, this will restart the NuGet release flow. Before retrying make sure to close any pull requests that might have been created by this release flow.

<!-- nuget-id: {{ .nugetId }} -->
<!-- nuget-version: {{ .nugetVersion}} -->
<!-- issue-nuget-release-node-status: {{ .issueNugetReleaseNodeStatus }} -->
<!-- issue-nuget-release-url: {{ .issueNugetReleaseUrl }} -->
<!-- nuget-release-pull-request-node-status: {{ .nugetReleasePullRequestNodeStatus }} -->
<!-- nuget-release-pull-request-url: {{ .nugetReleasePullRequestUrl }} -->
