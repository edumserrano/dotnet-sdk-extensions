namespace DotNet.Sdk.Extensions.Testing.Tests;

internal sealed class RunOnTargetFrameworkMajorVersionAttribute : FactAttribute
{
    public RunOnTargetFrameworkMajorVersionAttribute(params int[] targetFrameworkList)
    {
        TargetFrameworkList = targetFrameworkList;
        var runtimeMajorVersion = Environment.Version.Major;
        if (!targetFrameworkList.Contains(runtimeMajorVersion))
        {
            Skip = $"Skip on dotnet version {runtimeMajorVersion}. Allowed runtime major versions are: {string.Join(" or ", targetFrameworkList)}.";
        }
    }

    public int[] TargetFrameworkList { get; }
}
