# [{{ .workflow }}]({{ .workflowUrl }}) for commit {{ .commitSha }}

:exclamation: **dotnet format** found files that did not respect the code guidelines and pushed commit {{ .pushedCommitSha }}. Please review it before merging the PR. You can review the workflow that pushed this commit [here]({{ .pushWorkflowUrl }}).

:warning: dotnet format does **NOT** remove unused references. You have to do this manually. Please check that there isn't any unused reference.

<details>
<summary><strong>Note</strong></summary>
</br>

Sometimes the fix provided by the analyzers produces unnecessary comments when formatting files.

This should only happen if the project supports multiple target frameworks and the fix doesn't produce the same output for all. However, it seems that sometimes the `Unmerged change from project ...` comment shows up even though the fix produced the same output.

If this happens, just delete the comments added. Otherwise, consider incorporating the commented out code using [preprocessor directives to control conditional compilation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#conditional-compilation).
Example:

```csharp
#if NET6_0
    ...
#elif NET7_0
    ...
#endif
```

</details>

<!-- on-pr-dotnet-format -->
