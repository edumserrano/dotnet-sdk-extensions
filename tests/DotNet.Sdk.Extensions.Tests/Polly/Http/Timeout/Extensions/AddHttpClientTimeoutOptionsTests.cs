namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Extensions;

/// <summary>
/// Tests for the <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> method.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddHttpClientTimeoutOptionsTests
{
    /// <summary>
    /// Tests that the <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> extension method
    /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
    /// where TOptions is of type <see cref="TimeoutOptions"/>.
    ///
    /// It also checks that the <see cref="TimeoutOptions"/> has the expected values.
    /// It also tests the <see cref="TimeoutOptionsExtensions.GetHttpClientTimeoutOptions"/> extension method.
    /// </summary>
    [Fact]
    public void AddHttpClientTimeoutOptions()
    {
        const string optionsName = "timeoutOptions";
        const int timeoutInSecs = 3;
        var services = new ServiceCollection();
        services
            .AddHttpClientTimeoutOptions(optionsName)
            .Configure(options => options.TimeoutInSecs = timeoutInSecs);
        var serviceProvider = services.BuildServiceProvider();
        var timeoutOptions = serviceProvider.GetHttpClientTimeoutOptions(optionsName);
        timeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
    }
}
