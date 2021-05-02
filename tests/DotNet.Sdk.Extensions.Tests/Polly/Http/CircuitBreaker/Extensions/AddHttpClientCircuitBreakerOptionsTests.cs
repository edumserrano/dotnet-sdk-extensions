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
            var serviceProvider = services.BuildServiceProvider();
            var circuitBreakerOptions = serviceProvider.GetHttpClientCircuitBreakerOptions(optionsName);
            circuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            circuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            circuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            circuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method
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
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'FailureThreshold' with the error: 'The field FailureThreshold must be between {double.Epsilon} and {1}.'.");
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method
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
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'MinimumThroughput' with the error: 'The field MinimumThroughput must be between {2} and {int.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method
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
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'DurationOfBreakInSecs' with the error: 'The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method
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
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'SamplingDurationInSecs' with the error: 'The field SamplingDurationInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }
    }
}
