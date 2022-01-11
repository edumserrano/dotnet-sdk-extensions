# pr-dotnet-format-check workflow

[![PR dotnet format check](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml)

[This workflow](/.github/workflows/pr-dotnet-format-check.yml) runs [dotnet format](https://github.com/dotnet/format) with the `--verify-no-changes` flag and if there are any changes detected it then adds a comment to the pull request with the results of running the dotnet format tool.

The main idea is to notify that the pull request does not respect the code guidelines. The comment added to the PR will give instructions to the user on how to resolve the issue.

## Security considerations

This workflow uses the `pull_request_target` trigger and checks out the incoming code which could [lead to security vulnerabilities](./security-considerations.md#beware-of-pull_request_target). However since this workflow does NOT run any of the incoming code it is safe.
