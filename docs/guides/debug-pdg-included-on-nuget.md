# Debug NuGet package with included symbols

## Motivation

I want to publish symbols for my NuGet package to enable debugging of the package's code. Following [sourcelink's guide](https://github.com/dotnet/sourcelink) you can produce a NuGet package (.nupkg) and corresponding symbols package (.snupkg) without issues. You can then follow the following guides to debug the package's code:

- [Exploring .NET Core's SourceLink - Stepping into the Source Code of NuGet packages you don't own](https://www.hanselman.com/blog/exploring-net-cores-sourcelink-stepping-into-the-source-code-of-nuget-packages-you-dont-own)
- [How to Configure Visual Studio to Use SourceLink to Step into NuGet Package Source](https://aaronstannard.com/visual-studio-sourcelink-setup/)
- [Source Link - microsoft docs](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)

What is not straightforward is when you do NOT want to produce a separate symbols package and want to include the pdb in the NuGet package (.nupgk) itself. [SourceLink's documentation](https://github.com/dotnet/sourcelink#alternative-pdb-distribution) explains why you might want to do this and for my scenario the reason was that my NuGet server didn't support symbols packages and I didn't want to setup an alternate server just to store them.

So after following the [instructions to setup the up the NuGet package to include the pdb](https://github.com/dotnet/sourcelink#alternative-pdb-distribution) everything worked fine when producing the NuGet package but not when debugging. I found that I could NOT debug into the code of the NuGet package and the reason was because the pdb from the NuGet was not being copied to the output of the build which then meant the debugger would not load it.

*Note:* you can see which dlls and pdbs are loaded in Visual Studio by inspecting the modules window. To enable it run your application in Visual Studio and then go to `Debug->Windows->Modules` to open the modules window.

This problem is detailed at ["Alternative PDB distribution" (including .pdb in .nupkg) does not work when consumed in netcoreapp3.0+ projects](https://github.com/dotnet/sourcelink/issues/628). This issue also references the open issues on the dotnet repos that provide more information on the problem.

## How to

If you want to debug a NuGet package that has symbols included then you should add the following to the csproj files that use the package:

```
<!-- https://github.com/dotnet/sdk/issues/1458#issuecomment-420456386 -->
<Target Name="_ResolveCopyLocalNuGetPackagePdbs" Condition="$(CopyLocalLockFileAssemblies) == true" AfterTargets="ResolveReferences">
<ItemGroup>
    <ReferenceCopyLocalPaths
    Include="@(ReferenceCopyLocalPaths->'%(RootDir)%(Directory)%(Filename).pdb')"
    Condition="'%(ReferenceCopyLocalPaths.NuGetPackageId)' != '' and Exists('%(RootDir)%(Directory)%(Filename).pdb')" />
</ItemGroup>
</Target>
```

The above should be added inside the `Project` tag of the csproj and will make sure the pdb from the NuGet package is copied to the output when the project is built.

If you are also [producing documentation with your NuGet package](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc), including it with the NuGet package and the documentation is not showing up in intellisense then consider copying the documentation to the output directory as well by extending the above to:

```
<!-- https://github.com/dotnet/sdk/issues/1458#issuecomment-420456386 -->
<Target Name="_ResolveCopyLocalNuGetPackagePdbsAndXml" Condition="$(CopyLocalLockFileAssemblies) == true" AfterTargets="ResolveReferences">
    <ItemGroup>
        <ReferenceCopyLocalPaths
            Include="@(ReferenceCopyLocalPaths->'%(RootDir)%(Directory)%(Filename).pdb')"
            Condition="'%(ReferenceCopyLocalPaths.NuGetPackageId)' != '' and Exists('%(RootDir)%(Directory)%(Filename).pdb')" />
        <ReferenceCopyLocalPaths
            Include="@(ReferenceCopyLocalPaths->'%(RootDir)%(Directory)%(Filename).xml')"
            Condition="'%(ReferenceCopyLocalPaths.NuGetPackageId)' != '' and Exists('%(RootDir)%(Directory)%(Filename).xml')" />
    </ItemGroup>
</Target>
```

## Apply this setting to multiple projects

If you want to apply the above to multiple projects at once you can do it without having to make changes on each individual csproj if you create a `Directory.Build.targets` or a `Directory.Build.props` and adding the following to it:

```
<Project>
  <!-- https://github.com/dotnet/sdk/issues/1458#issuecomment-420456386 -->
  <Target Name="_ResolveCopyLocalNuGetPackagePdbsAndXml" Condition="$(CopyLocalLockFileAssemblies) == true" AfterTargets="ResolveReferences">
    <ItemGroup>
      <ReferenceCopyLocalPaths
        Include="@(ReferenceCopyLocalPaths->'%(RootDir)%(Directory)%(Filename).pdb')"
        Condition="'%(ReferenceCopyLocalPaths.NuGetPackageId)' != '' and Exists('%(RootDir)%(Directory)%(Filename).pdb')" />
      <ReferenceCopyLocalPaths
        Include="@(ReferenceCopyLocalPaths->'%(RootDir)%(Directory)%(Filename).xml')"
        Condition="'%(ReferenceCopyLocalPaths.NuGetPackageId)' != '' and Exists('%(RootDir)%(Directory)%(Filename).xml')" />
    </ItemGroup>
  </Target>
</Project>
```

For more information on these customization files see:

- [Directory.Build.props and Directory.Build.targets](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build?view=vs-2019#directorybuildprops-and-directorybuildtargets)
- [Difference between Directory.Build.props and .targets is not clear](https://github.com/MicrosoftDocs/visualstudio-docs/issues/2774#issuecomment-489685855)