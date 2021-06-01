using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Extensions
{
    /// <summary>
    /// Tests for the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> method
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddHttpClientCircuitBreakerOptionsTests
    {
        /// <summary>
        /// Tests that the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method
        /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
        /// where TOptions is of type <see cref="CircuitBreakerOptions"/>.
        ///
        /// It also checks that the <see cref="CircuitBreakerOptions"/> has the expected values.
        /// It also tests the <see cref="CircuitBreakerOptionsExtensions.GetHttpClientCircuitBreakerOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddHttpClientCircuitBreakerOptions()
        {
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                });
            using var serviceProvider = services.BuildServiceProvider();
            var circuitBreakerOptions = serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            circuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            circuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            circuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            circuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
        }
    }
}
