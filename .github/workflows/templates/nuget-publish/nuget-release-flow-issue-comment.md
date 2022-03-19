<!-- nuget-release-flow -->
## NuGet release flow progress

[![Generic badge](https://img.shields.io/badge/status-{{ .releaseStatus }}-{{ .releaseBadgeColor }}.svg)](https://shields.io/)

``````mermaid
  graph TD
      A([Create NuGet release issue]):::ok --> B
      B[Action: Issue for NuGet release]:::ok --> C
      C[Action: NuGet release command handler]:::ok --> D
      D[Pull Request: Release NuGet {{ .nugetId }} {{ .nugetVersion }}]:::ok --> E
      E[Action: Publish NuGet packages]:::{{ .graphNodeStatus }} --> F([NuGet released]):::{{ .graphNodeStatus }}
  classDef ok stroke:#a5e16e 
  classDef error stroke:#ff1355
  classDef inProgress stroke:#2986cc
  click B "{{ .issueNuGetReleaseUrl }}" _blank
  click C "{{ .nugetReleaseCommandHandlerUrl }}" _blank
  click D "{{ .nugetReleasePullRequestUrl }}" _blank
  click E "{{ .publishNugetUrl }}" _blank
``````

- This shows the overall progress of the NuGet release flow. This is updated in real time, even if you retry the release.
- If any step fails it will be highlighted in red. Click on it to find out more details in the logs of the action.
- To retry close and and reopen this issue, this will restart the NuGet release flow. Before retrying make sure to close any pull requests that might have been created by this release flow.

<!-- issue-nuget-release-url: {{ .issueNuGetReleaseUrl }} -->
<!-- nuget-release-command-handler-url: {{ .nugetReleaseCommandHandlerUrl }} -->
<!-- nuget-release-pull-request-url: {{ .nugetReleasePullRequestUrl }} -->
<!-- publish-nuget-url: {{ .publishNugetUrl }} -->
