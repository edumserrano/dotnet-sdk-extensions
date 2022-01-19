# [{{ .workflow }}]({{ .workflowUrl }}) for commit {{ .commitSha }}

**dotnet format** detected code guidelines violations and automatically created this PR.

:warning: Please review the suggested changes before merging.

<details>
<summary><strong>Note</strong></summary>
</br>

Sometimes the fix provided by the analyzers produces unnecessary comments when formatting files.

This should only happen if the project supports multiple target frameworks and the fix doesn't produce the same output for all. However, it seems that sometimes the `Unmerged change from project ...` comment shows up even though the fix produced the same output.

If this happens, just delete the comments added. Otherwise, consider incorporating the commented out code using [preprocessor directives to control conditional compilation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#conditional-compilation).
Example:

````````````csharp
#if NET5_0
    ...
#elif NETCOREAPP3_1
    ...
#endif
````````````

</details>
