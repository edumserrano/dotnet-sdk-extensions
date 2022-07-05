using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions;

/// <summary>
/// Tests for the <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> method.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddHttpClientRetryOptionsTests
{
    /// <summary>
    /// Tests that the <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> extension method
    /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
    /// where TOptions is of type <see cref="RetryOptions"/>.
    ///
    /// It also checks that the <see cref="RetryOptions"/> has the expected values.
    /// It also tests the <see cref="RetryOptionsExtensions.GetHttpClientRetryOptions"/> extension method.
    /// </summary>
    [Fact]
    public void AddHttpClientRetryOptions()
    {
        const string optionsName = "retryOptions";
        const int retryCount = 3;
        const int medianFirstRetryDelayInSecs = 1;
        var services = new ServiceCollection();
        services
            .AddHttpClientRetryOptions(optionsName)
            .Configure(options =>
            {
                options.RetryCount = retryCount;
                options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
            });
        var serviceProvider = services.BuildServiceProvider();
        var retryOptions = serviceProvider.GetHttpClientRetryOptions(optionsName);
        retryOptions.RetryCount.ShouldBe(retryCount);
        retryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
    }
}
