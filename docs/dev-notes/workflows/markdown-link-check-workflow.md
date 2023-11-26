# markdown-link-check workflow

[![Markdown link check](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/markdown-link-check.yml)

[This workflow](/.github/workflows/markdown-link-check.yml) analyzes all the markdown files in the repository and looks for any broken links. The configuration is done via the [markdown-link-check-config.json](/.github/markdown-link-check-config.json) file. Each configuration block has a description property that, albeit not being part of the schema of the configuration file, helps document the intention for the configuration block.

When the workflow completes, it uploads a workflow info artifact that contains a summary `json` file which indicates if the there are any broken links as well as some other required info that will be further processed by the [markdown-link-check-handle-result workflow](/docs/dev-notes/workflows/markdown-link-check-handle-result-workflow.md).

> [!NOTE]
>
> The reason to split this workflow in two, this one that looks for broken links `(markdown-link-check)` and one that processes the results `(markdown-link-check-handle-result)` is due to security. On GitHub, workflows that run on PRs from forks of the repo run in a restricted context without access to secrets and where the `GITHUB_TOKEN` has read-only permissions. The main purpose for this is to protect from the threat of malicious pull requests.
>
> Without doing this, even if security wasn't an issue, the PRs from forked repos would fail when the `markdown-link-check` workflow tried to add a comment to a PR or create an issue. Both of these actions are part of the flow to detect broken markdown links and are executed by the [markdown-link-check-handle-result workflow](/docs/dev-notes/workflows/markdown-link-check-handle-result-workflow.md) running on a priviliged context.
>
> For more information see:
>
> - [Security considerations on GitHub workflows](/docs/dev-notes/workflows/security-considerations.md)
> - [Security considerations on GitHub workflows regarding dotnet CLI](/docs/dev-notes/workflows/security-considerations-and-dotnet.md)
