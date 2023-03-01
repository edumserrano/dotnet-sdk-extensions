namespace DotNet.Sdk.Extensions.Testing.Tests;

internal sealed class RunOnlyOnDotnet6And7Attribute : FactAttribute
{
#if NET6_0 || NET7_0
    public RunOnlyOnDotnet6And7Attribute()
    {
        Skip = "Runs only on net6 and net7.";
    }
#endif
}
