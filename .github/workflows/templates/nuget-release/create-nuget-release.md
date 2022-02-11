Create {{ .nugetNewVersion }} release for {{ .projName }} NuGet.
Current version of {{ .projName }} NuGet is: [{{ .nugetCurrentVersion }}]({{ .nugetUrl }}).

Release notes can be found at #{{ .issueNumber }}.

<details>
<summary><strong>Created from:</strong></summary>
</br>

Issue: #{{ .issueNumber }}
Workflow: [{{ .workflow }}](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/dispatch-commands.yml)
Commmit: {{ .commitSha }}

</details>
