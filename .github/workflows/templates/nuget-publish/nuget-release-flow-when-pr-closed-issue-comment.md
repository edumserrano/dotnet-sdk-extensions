<!-- nuget-release-flow -->
## NuGet release flow progress

[![Generic badge](https://img.shields.io/badge/status-{{ .releaseStatus }}-{{ .releaseBadgeColor }}.svg)](https://shields.io/)

``````mermaid
  graph TD
      A([Create NuGet release issue]):::ok --> B
      B[Action: Issue for NuGet release]:::ok --> C
      C[Workflow: NuGet release command handler]:::ok --> D
      D[Pull Request: Release NuGet {{ .nugetId }} {{ .nugetVersion }}]:::{{ .graphNodeStatus }} --> E
      E[Workflow: Publish NuGet packages] --> F([NuGet released])
  classDef ok stroke:#a5e16e 
  classDef error stroke:#ff1355
``````

- This shows the overall progress of the NuGet release flow. This is updated in real time, even if you retry the release.
- If any step fails it will be highlighted in red. Click on it to find out more details from the action's logs.
- To retry close and and reopen this issue, this will restart the NuGet release flow. Before retrying make sure to close any pull requests that might have been created by this release flow.
