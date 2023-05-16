# markdown-link-check-handle-result workflow

[![Markdown link check - broken links](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check-handle-result.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check-handle-result.yml)

[This workflow](/.github/workflows/markdown-link-check-handle-result.yml):

- Processes the result of the [markdown-link-check workflow](/docs/dev-notes/workflows/markdown-link-check-workflow.md).
- Run a report on the markdown link check executed by the markdown-link-check workflow and display the result on this workflow summary.
- Parses the workflow info artifact uploaded to the markdown-link-check workflow. This will contain information that allows to understand if, for instance, there are broken markdown links or not.

If the markdown-link-check workflow was executed on the main branch:

- If there aren't any broken markdown links then the workflow stops.
- If there is any markdown link check issue already open then the workflow stops. Markdown link check issues are identified by the `markdown-link-check` label. No point in creating another issue if one is already open The issue contains information about the links that are broken and even if new broken links were found since the issue was created that information won't be lost. Once the issue is corrected and a new push to the main branch is done the process restarts and if there are still broken markdown links a new issue will be created.
- If there isn't any open markdown link check issue then one will be created with information about the broken markdown links.

If the markdown-link-check workflow was executed on a pull request branch:

- If there aren't any broken markdown links then the workflow stops.
- If there are broken markdown links then a comment with information about the broken links will be added to the Pull Request.

> **Note**
>
> The reason to split this workflow in two, one that looks for broken links `(markdown-link-check)` and this one that processes the results `(markdown-link-check-handle-result)` is due to security. On GitHub, workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions. The main purpose for this is to protect from the threat of malicious pull requests.
>
> Without doing this, even if security wasn't an issue, the PRs from forked repos would fail when the `markdown-link-check` workflow tried to add a comment to a PR or create an issue. Both of these actions are part of the flow to detect broken markdown links and are executed by this workflow running on a priviliged context.
>
> For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
