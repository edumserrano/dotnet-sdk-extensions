# codeql workflow

[![CodeQL](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/codeql.yml/badge.svg)](https://github.com/edumserrano/dotnet-sdk-extensions/actions/workflows/codeql.yml)

[This workflow](/.github/workflows/codeql.yml) performs [code scanning with CodeQL](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/about-code-scanning). The results are uploaded to the repo and visible on [code scanning alerts](https://github.com/edumserrano/dotnet-sdk-extensions/security/code-scanning). The resulting [`SARIF`](https://sarifweb.azurewebsites.net/) file is also uploaded as a workflow artifact.

When doing pull requests the alerts detected will be visible on via [file annotations](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/triaging-code-scanning-alerts-in-pull-requests).

This workflow produces status checks on pull requests and the repo is configured so that the status check fails if [any alert](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/configuring-code-scanning#defining-the-alert-severities-causing-pull-request-check-failure) is found.

## About using on:push paths-ignore

Initially this workflow was configured to ignore some paths but [as per documentation](https://docs.github.com/en/code-security/secure-coding/automatically-scanning-your-code-for-vulnerabilities-and-errors/configuring-code-scanning#avoiding-unnecessary-scans-of-pull-requests):
>For CodeQL code scanning workflow files, don't use the paths-ignore or paths keywords with the on:push event as this is likely to cause missing analyses. For accurate results, CodeQL code scanning needs to be able to compare new changes with the analysis of the previous commit.

This is a [broken link test](https://docs.github.com/en/what-is-going-on)
