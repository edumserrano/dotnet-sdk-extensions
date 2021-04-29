using System.ComponentModel.DataAnnotations;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience
{
    /// <summary>
    /// Tests for the <see cref="ResilienceOptionsExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class ResilienceOptionsExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
        /// where TOptions is of type <see cref="ResilienceOptions"/>.
        ///
        /// It also checks that the <see cref="ResilienceOptions"/> has the expected values.
        /// </summary>
        [Fact]
        public void AddHttpClientResilienceOptions()
        {
            var optionsName = "resilienceOptions";
            var timeoutInSecs = 2;
            var medianFirstRetryDelayInSecs = 1;
            var retryCount = 3;
            var durationOfBreakInSecs = 4;
            var failureThreshold = 0.5;
            var samplingDurationInSecs = 60;
            var minimumThroughput = 5;
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
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="ResilienceOptions.Timeout"/> cannot be null.
        /// </summary>
        [Fact]
        public void AddHttpClientResilienceOptionsValidatesTimeoutOptionsNull()
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout = null!;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe("The Timeout field is required.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="TimeoutOptions.TimeoutInSecs"/>. Can only be positive value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientResilienceOptionsValidatesTimeoutOptions(double timeoutInSecs)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = timeoutInSecs;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="ResilienceOptions.Retry"/> cannot be null.
        /// </summary>
        [Fact]
        public void AddHttpClientResilienceOptionsValidatesRetryOptionsNull()
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry = null!;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe("The Retry field is required.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="RetryOptions.MedianFirstRetryDelayInSecs"/>.
        /// Can only be a positive number.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientResilienceOptionsValidatesRetryOptions(double medianFirstRetryDelayInSecs)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field MedianFirstRetryDelayInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="RetryOptions.RetryCount"/>. Needs to be a number >= 0.
        /// 
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void AddHttpClientResilienceOptionsValidatesRetryOptions2(int retryCount)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = retryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field RetryCount must be between {0} and {int.MaxValue}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="RetryOptions.RetryCount"/> can be zero.
        /// </summary>
        [Fact]
        public void AddHttpClientResilienceOptionsValidatesRetryOptions3()
        {
            var optionsName = "resilienceOptions";
            var retryCount = 0;
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = retryCount;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider
                .GetHttpClientResilienceOptions(optionsName)
                .ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="ResilienceOptions.CircuitBreaker"/> cannot be null.
        /// </summary>
        [Fact]
        public void AddHttpClientResilienceOptionsValidatesCircuitBreakerOptionsNull()
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 2;
                    options.CircuitBreaker = null!;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe("The CircuitBreaker field is required.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.FailureThreshold"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientResilienceOptionsValidatesCircuitBreakerOptions(double failureThreshold)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = failureThreshold;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field FailureThreshold must be between {double.Epsilon} and {1}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.MinimumThroughput"/>. Can only
        /// be a number >= 2.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        public void AddHttpClientResilienceOptionsValidatesCircuitBreakerOptions2(int minimumThroughput)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = minimumThroughput;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field MinimumThroughput must be between {2} and {int.MaxValue}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientResilienceOptionsValidatesCircuitBreakerOptions3(double durationOfBreakInSecs)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> extension method
        /// validates the <see cref="CircuitBreakerOptions.SamplingDurationInSecs"/>. Can only
        /// be a number >= 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientResilienceOptionsValidatesCircuitBreakerOptions4(double samplingDurationInSecs)
        {
            var optionsName = "resilienceOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 4;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = samplingDurationInSecs;
                    options.CircuitBreaker.MinimumThroughput = 5;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                return serviceProvider.GetHttpClientResilienceOptions(optionsName);
            });
            exception.Message.ShouldBe($"The field SamplingDurationInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }
    }
}
