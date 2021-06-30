using System.ComponentModel.DataAnnotations;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically for the <see cref="ResilienceOptions.CircuitBreaker"/> validation.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesCircuitBreakerOptionsValidationTests
    {
        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="ResilienceOptions.Retry"/> cannot be null.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidationForRetryOptions()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.CircuitBreaker = null!;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("The CircuitBreaker field is required.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="CircuitBreakerOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> needs to be a double > 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        [InlineData(-3.5)]
        public void AddResiliencePoliciesOptionsValidationForDurationOfBreakInSecs(double durationOfBreakInSecs)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = 10;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions.CircuitBreaker"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="CircuitBreakerOptions.SamplingDurationInSecs"/> needs to be a double > 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        [InlineData(-3.5)]
        public void AddResiliencePoliciesOptionsValidationForSamplingDurationInSecs(double samplingDurationInSecs)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.SamplingDurationInSecs = samplingDurationInSecs;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = 10;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field SamplingDurationInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions.CircuitBreaker"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="CircuitBreakerOptions.FailureThreshold"/> needs to be a double > 0
        /// and less than or equal to 1.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(1.1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddResiliencePoliciesOptionsValidationForFailureThreshold(double failureThreshold)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = failureThreshold;
                    options.CircuitBreaker.MinimumThroughput = 10;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field FailureThreshold must be between {double.Epsilon} and {1}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions.CircuitBreaker"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="CircuitBreakerOptions.MinimumThroughput"/> needs to be an int >= 2.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddResiliencePoliciesOptionsValidationForMinimumThroughput(int minimumThroughput)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = minimumThroughput;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field MinimumThroughput must be between {2} and {int.MaxValue}.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> and those
        /// option configurations will be honored.
        ///
        /// In this test we configure the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> to 1 and
        /// force a validation that this value must be > 3.
        /// Although the default data annotation validations only enforces that the value must be >= 0, with the
        /// extra validation the options validation will fail.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation2()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = 10;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                })
                .Validate(options =>
                {
                    return options.CircuitBreaker.DurationOfBreakInSecs > 3;
                });
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(optionsName);

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> and
        /// those option configurations will be honored.
        ///
        /// In this test we configure the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> to -1 and
        /// force a validation that this value must be > 3.
        /// With this setup both the default data annotation validation and the custom one will be triggered.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation3()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.CircuitBreaker.DurationOfBreakInSecs = -1;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = 10;
                    options.EnableRetryPolicy = false;
                    options.EnableTimeoutPolicy = false;
                })
                .Validate(options =>
                {
                    return options.CircuitBreaker.DurationOfBreakInSecs > 3;
                });
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(optionsName);

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                _ = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// don't validate the <see cref="ResilienceOptions.CircuitBreaker"/> when the <see cref="ResilienceOptions.EnableCircuitBreakerPolicy"/>.
        /// is false.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation4()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.EnableRetryPolicy = false;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                    options.CircuitBreaker.DurationOfBreakInSecs = -1; // this should cause validation to fail if the EnableRetryPolicy was set to true
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.FailureThreshold = 0.6;
                    options.CircuitBreaker.MinimumThroughput = 10;
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = Should.NotThrow(() => serviceProvider.InstantiateNamedHttpClient(httpClientName));
        }
    }
}
