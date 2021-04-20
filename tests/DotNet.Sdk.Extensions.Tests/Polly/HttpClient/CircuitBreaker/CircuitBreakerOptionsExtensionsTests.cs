using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.CircuitBreaker
{
    /// <summary>
    /// Tests for the <see cref="CircuitBreakerOptionsExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class CircuitBreakerOptionsExtensionsTests
    {
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
            var serviceProvider = services.BuildServiceProvider();
            var circuitBreakerOptions = serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            circuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            circuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            circuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            circuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
        }

        /// <summary>
        /// Tests that the <see cref="AddHttpClientCircuitBreakerOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.FailureThreshold"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientCircuitBreakerOptionsValidatesOptions(double failureThreshold)
        {
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
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
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'FailureThreshold' with the error: 'The field FailureThreshold must be between 5E-324 and 1.'.");
        }

        /// <summary>
        /// Tests that the <see cref="AddHttpClientCircuitBreakerOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.MinimumThroughput"/>. Can only
        /// be a number >= 2.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        public void AddHttpClientCircuitBreakerOptionsValidatesOptions2(int minimumThroughput)
        {
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
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
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'MinimumThroughput' with the error: 'The field MinimumThroughput must be between 2 and 2147483647.'.");
        }

        /// <summary>
        /// Tests that the <see cref="AddHttpClientCircuitBreakerOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientCircuitBreakerOptionsValidatesOptions3(double durationOfBreakInSecs)
        {
            var optionsName = "circuitBreakerOptions";
            var failureThreshold = 0.5;
            var samplingDurationInSecs = 60;
            var minimumThroughput = 2;
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
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'DurationOfBreakInSecs' with the error: 'The field DurationOfBreakInSecs must be between 5E-324 and 1.7976931348623157E+308.'.");
        }

        /// <summary>
        /// Tests that the <see cref="AddHttpClientCircuitBreakerOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.SamplingDurationInSecs"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientCircuitBreakerOptionsValidatesOptions4(double samplingDurationInSecs)
        {
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 2;
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
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'SamplingDurationInSecs' with the error: 'The field SamplingDurationInSecs must be between 5E-324 and 1.7976931348623157E+308.'.");
        }
    }
}
