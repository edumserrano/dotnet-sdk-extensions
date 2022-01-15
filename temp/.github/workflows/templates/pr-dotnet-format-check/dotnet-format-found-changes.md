# [{{ .workflow }}]({{ .workflowUrl }}) for commit {{ .commitSha }}

:exclamation: **dotnet format** found files that do not respect the code guidelines.

Before merging this PR please fix the reported issues. You can do so by either:

- adding a comment on this PR with:

``````
/dotnet-format
``````

- or locally running [dotnet format](https://github.com/dotnet/format) on the solution as such:

``````
dotnet format DotNet.Sdk.Extensions.sln --severity info --verbosity diagnostic
``````

:warning: dotnet format does **NOT** remove unused references. You have to do this manually. Please check that there isn't any unused reference.
