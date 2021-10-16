# nuget-publish workflow

[![Publish Nuget packages](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/edumserrano/dot-net-sdk-extensions/actions/workflows/nuget-publish.yml)

[This workflow]((/.github/workflows/nuget-publish.yml)) will:

1) Always builds the code and run tests on windows-latest and ubuntu-latest.
2) On ubuntu-latest, generates code coverage and uploads it to Codecov and as a workflow artifact.
3) On ubuntu-latest, generates NuGet packages and uploads them as a workflow artifact.
4) Publishes NuGet packages if the workflow was not triggered by a pull request.

## Secrets

This workflow uses a custom secret `NUGET_PUSH_API_KEY`. The secret contains a NuGet API key that is used to publish the NuGet packages. This API key will expire on the 5th February 2022 and will have to be renewed before that or the workflow will fail.

## Codecov

Codecov integration does not require any secret, it was done via the [Codecov GitHub app](https://github.com/apps/codecov).

Besides the information available on the [Codecov web app](https://app.codecov.io/gh/edumserrano/dot-net-sdk-extensions), this integration enables Codecov to:

- [add status checks on pull requests](https://docs.codecov.com/docs/commit-status)
- [display coverage on pull requests via comments](https://docs.codecov.com/docs/pull-request-comments)
- [add line-by-line coverage on pull requests via file annotations](https://docs.codecov.com/docs/github-checks)

The [Codecov configuration file](/.github/codecov.yml) contains additional configuration for Codecov.

## Build warnings will make the workflow fail

The `dotnet build` command includes the `-warnaserror` flag which will cause the build to fail if there are any errors.

This is used to help keep the code healthy whilst balancing local dev. Meaning, when developing locally there is no need to force all warnings to be fixed to be able to build the code.

## NuGet push action

Initially the NuGet package and symbols push step was being done by:

```
dotnet nuget push ./*.nupkg --api-key <api-key> --source https://api.nuget.org/v3/index.json --skip-duplicate
```

This step had a flag set to skip publishing the NuGet (.nupgk) if the version has already been publish. This allowed the workflow to run without failing even if we didn't want to publish a new version of the package.

However there was an issue with this approach in that even if the NuGet package already existed the `--skip-duplicate` flag only makes it so that the `nuget push` command doesn't fail due to the returned 409 from the server but it still tries to push the symbols package after.

The above doesn't fail but it makes nuget.org send emails to the owner of the package with the following:

```
Symbols package publishing failed. The associated symbols package could not be published due to the following reason(s):
The uploaded symbols package contains pdb(s) for a corresponding dll(s) not found in the NuGet package.
Once you've fixed the issue with your symbols package, you can re-upload it.

Please note: The last successfully published symbols package is still available for debugging and download.
```

The above error message is also displayed on the NuGet's package page even though it's only visible to the owner of the package.

For more information about this see:

- [dotnet nuget push with --skip-duplicate pushes .snupkg constantly and causes validation to fail.](https://github.com/NuGet/Home/issues/10475)
- [When nupkg exists on push --skip-duplicate, don't automatically push snupkg](https://github.com/NuGet/Home/issues/9647)
- [[Symbols] Support removing snupkg validation error messages](https://github.com/NuGet/NuGetGallery/issues/8036)

To avoid this happening I created [a nuget push action](/.github/actions/nuget-push/action.yml) that:

1. Tries to do a push only of the `nuget package` and only if it succeeds attempts to do a following push of the symbols package.
2. If the `nuget push` returns a 409 because the package exists that is outputted by the action and the symbols push is skipped.
3. If the `nuget push` fails because of any other reason the output from `nuget push` is shown by the action and the symbols push is skipped.

The action was created following the guidance from [Creating a composite run steps action](https://docs.github.com/en/actions/creating-actions/creating-a-composite-run-steps-action). Even though GitHub's action docs don't mention how to reference an action from a local repo, it's possible by using the full path to the action.

Also note that the filename for the custom action must be `action.yml`.
