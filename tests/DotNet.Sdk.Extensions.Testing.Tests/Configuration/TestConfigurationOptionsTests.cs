namespace DotNet.Sdk.Extensions.Testing.Tests.Configuration;

[Trait("Category", XUnitCategories.Configuration)]
public class TestConfigurationOptionsTests
{
    /// <summary>
    /// Tests the default values for <see cref="TestConfigurationOptions"/>.
    /// </summary>
    [Fact]
    public void DefaultValues()
    {
        var options = new TestConfigurationOptions();
        options.IsRelative.ShouldBe(true);
        options.AppSettingsDir.ShouldBe("AppSettings");
    }
}
