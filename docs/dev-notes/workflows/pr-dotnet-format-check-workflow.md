# pr-dotnet-format-check workflow

[![PR dotnet format check](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/pr-dotnet-format-check.yml)

[This workflow](/.github/workflows/pr-dotnet-format-check.yml) runs [dotnet format](https://github.com/dotnet/format) with the `--check` flag so that if there are any changes detected it then adds a comment to the pull request with the results of running the dotnet format tool.
