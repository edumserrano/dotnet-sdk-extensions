using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions;

/// <summary>
/// Tests for the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> method.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddHttpClientResilienceOptionsTests
{
    /// <summary>
    /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
    /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
    /// where TOptions is of type <see cref="ResilienceOptions"/>.
    ///
    /// It also checks that the <see cref="ResilienceOptions"/> has the expected values.
    /// It also tests the <see cref="ResilienceOptionsExtensions.GetHttpClientResilienceOptions"/> extension method.
    /// </summary>
    [Fact]
    public void AddHttpClientResilienceOptions1()
    {
        const string optionsName = "resilienceOptions";
        const int timeoutInSecs = 2;
        const int medianFirstRetryDelayInSecs = 1;
        const int retryCount = 3;
        const int durationOfBreakInSecs = 4;
        const double failureThreshold = 0.5;
        const int samplingDurationInSecs = 60;
        const int minimumThroughput = 5;
        var services = new ServiceCollection();
        services
            .AddHttpClientResilienceOptions(optionsName)
            .Configure(options =>
            {
                options.Timeout.TimeoutInSecs = timeoutInSecs;
                options.Retry.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                options.Retry.RetryCount = retryCount;
                options.CircuitBreaker.DurationOfBreakInSecs = durationOfBreakInSecs;
                options.CircuitBreaker.FailureThreshold = failureThreshold;
                options.CircuitBreaker.SamplingDurationInSecs = samplingDurationInSecs;
                options.CircuitBreaker.MinimumThroughput = minimumThroughput;
            });
        var serviceProvider = services.BuildServiceProvider();
        var resilienceOptions = serviceProvider.GetHttpClientResilienceOptions(optionsName);
        resilienceOptions.Timeout.TimeoutInSecs.ShouldBe(timeoutInSecs);
        resilienceOptions.Retry.RetryCount.ShouldBe(retryCount);
        resilienceOptions.Retry.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
        resilienceOptions.CircuitBreaker.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
        resilienceOptions.CircuitBreaker.FailureThreshold.ShouldBe(failureThreshold);
        resilienceOptions.CircuitBreaker.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
        resilienceOptions.CircuitBreaker.MinimumThroughput.ShouldBe(minimumThroughput);
    }

    /// <summary>
    /// Tests default values for <see cref="ResilienceOptions"/>.
    /// </summary>
    [Fact]
    public void ResilienceOptionsTest1()
    {
        var options = new ResilienceOptions();
        options.EnableFallbackPolicy.ShouldBeTrue();
        options.EnableCircuitBreakerPolicy.ShouldBeTrue();
        options.EnableRetryPolicy.ShouldBeTrue();
        options.EnableTimeoutPolicy.ShouldBeTrue();
        options.CircuitBreaker.ShouldNotBeNull();
        options.Retry.ShouldNotBeNull();
        options.Timeout.ShouldNotBeNull();
    }
}
