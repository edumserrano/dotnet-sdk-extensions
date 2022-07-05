using DotNet.Sdk.Extensions.Testing.HostedServices;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilOptionsTests
{
    /// <summary>
    /// Tests the default values for <see cref="RunUntilOptions"/>.
    /// </summary>
    [Fact]
    public void DefaultValues()
    {
        var options = new RunUntilOptions();
        options.PredicateCheckInterval.ShouldBe(TimeSpan.FromMilliseconds(5));
        options.Timeout.ShouldBe(TimeSpan.FromSeconds(5));
    }
}
