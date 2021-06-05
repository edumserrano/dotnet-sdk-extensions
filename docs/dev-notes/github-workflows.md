# GitHub worflows

There are two workflows setup on this repo:

| Worflow                   |      Status and link      |
|---------------------------|:-------------------------:|
| [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/main/.github/workflows/nuget-publish.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Publish%20Nuget%20packages/badge.svg) |


## Notes about the worlflow

### Secrets

The [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/main/.github/workflows/nuget-publish.yml) workflow uses one secret for the NuGet API key used to publish the NuGet packages.

This API key will expire on  and will have to be renewed before 5th February 2022 or the workflwo will fail.

### Nuget push action

Initially the nuget package and symbols push step was being done by:

```
dotnet nuget push ./*.nupkg --api-key <api-key> --source https://api.nuget.org/v3/index.json --skip-duplicate'
```

This step had a flag set to skip publishing the NuGet (.nupgk) if the version has already been publish. This allowed the workflow to run without failing even if we didn't want to publish a new version of the package.

However there was an issue with this approach in that even if the nuget package already existed the '--skip-duplicate' flag only makes it so that the nuget push command doesn't fail due to the returned 409 from the server but it still tries to push the symbols package after.

The above doesn't fail but it makes NuGet.org send emails to the owner of the package with the following:
 
```
Symbols package publishing failed. The associated symbols package could not be published due to the following reason(s):
The uploaded symbols package contains pdb(s) for a corresponding dll(s) not found in the nuget package.
Once you've fixed the issue with your symbols package, you can re-upload it.

Please note: The last successfully published symbols package is still available for debugging and download.
```

The above error message is also displayed on the nuget's package page even though it's only visible to the owner of the package.

For more information about this see:

- [dotnet nuget push with --skip-duplicate pushes .snupkg constantly and causes validation to fail.](https://github.com/NuGet/Home/issues/10475)
- [When nupkg exists on push --skip-duplicate, don't automatically push snupkg](https://github.com/NuGet/Home/issues/9647)
- [[Symbols] Support removing snupkg validation error messages](https://github.com/NuGet/NuGetGallery/issues/8036)

To avoid this happening I created [a nuget push action](/.github/actions/nuget-push/action.yml) that:

1. Tries to do a push only of the nuget package and only if it succeeds attempts to do a following push of the symbols package.
2. If the nuget push returns a 409 because the package exists that is outputted by the action and the symbols push is skipped.
3. If the nuget push fails because of any other reason the output from nuget push is outputted by the action and the symbols push is skipped.

The action was created following the guidance from [Creating a composite run steps action](https://docs.github.com/en/actions/creating-actions/creating-a-composite-run-steps-action). Even though GitHub's action docs don't mention how to reference an action from a local repo, it's possible by using the full path to the action.

Also note that the filename for the custom action must be `action.yml`.
