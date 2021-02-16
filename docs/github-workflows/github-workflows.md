# GitHub worflows

There are two workflows setup on this repo:

| Worflow                   |      Status and link      |
|---------------------------|:-------------------------:|
| [build-demos](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/build-demos.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Build%20demos/badge.svg) |
| [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/nuget-publish.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Publish%20Nuget%20packages/badge.svg) |


## Notes for repo owner

### Secrets

The [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/nuget-publish.yml) workflow uses one secret for the NuGet API key used to publish the NuGet packages.

This API key will expire on  and will have to be renewed before 5th February 2022 or the workflwo will fail.

### Symbols package warning

When running the [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/nuget-publish.yml) workflow there's a final step to publish the NuGet packages (.nupkg) and respective symbols (.snupkg).

This step has a flag set to skip publishing the NuGet (.nupgk) if the version has already been publish. This allows the workflow to run without failing even if we don't want to publish a new version of the package.

However there is currently an issue with this in that the symbols package (.snupkg) for the NuGet package always gets pushed no matter what and this causes the validation of the symbols package to fail because the signature of the symbols package does not match the signature of the dll on the existing NuGet package.

This happens even if there is no code change. You can only see this validation error when logging in to NuGet.org and browsing to your package (you will also receive an email). This is safe to ignore because the first time a new version is published the symbols will be valid and that symbols package will be served even when this validation error starts occurring on subsquent runs of the workflow for the same version of the NuGet.

For more info see:

- [Error while publishing snupkg package](https://github.com/NuGet/NuGetGallery/issues/7949)
- [[Symbols] Support removing snupkg validation error messages](https://github.com/NuGet/NuGetGallery/issues/8036)