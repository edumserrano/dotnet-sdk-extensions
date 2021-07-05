using System.ComponentModel.DataAnnotations;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically for the <see cref="ResilienceOptions.Retry"/> validation.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesRetryOptionsValidationTests
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
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.Retry = null!;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("The Retry field is required.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions.Retry"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.RetryCount"/> needs to be an int >= 0.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        public void AddResiliencePoliciesOptionsValidationForRetryCount(int retryCount)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.Retry.RetryCount = retryCount;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field RetryCount must be between {0} and {int.MaxValue}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="RetryOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.RetryCount"/> can be zero.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidationForRetryCount2()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.Retry.RetryCount = 0;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;

                    options.Timeout.TimeoutInSecs = 1;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                });

            var serviceProvider = services.BuildServiceProvider();
            Should.NotThrow(() => serviceProvider.InstantiateNamedHttpClient(httpClientName));
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// validate the <see cref="ResilienceOptions.Retry"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.MedianFirstRetryDelayInSecs"/> needs to be a double > 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        [InlineData(-3.5)]
        public void AddResiliencePoliciesOptionsValidationForMedianFirstRetryDelayInSecs(int medianFirstRetryDelayInSecs)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.Retry.RetryCount = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;

                    options.Timeout.TimeoutInSecs = 1;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field MedianFirstRetryDelayInSecs must be between {double.Epsilon} and {double.MaxValue}.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.RetryCount"/> to 2 and force a validation
        /// that this value must be > 3.
        /// Although the default data annotation validations only enforces that the value must be >= 0, with the
        /// extra validation the options validation will fail.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation2()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Retry.RetryCount = 2;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                })
                .Validate(options =>
                {
                    return options.Retry.RetryCount > 3;
                });
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.RetryCount"/> to -1 and force a validation
        /// that this value must be > 3.
        /// With this setup both the default data annotation validation and the custom one will be triggered.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation3()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Retry.RetryCount = -1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                })
                .Validate(options =>
                {
                    return options.Retry.RetryCount > 3;
                });
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<ValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"The field RetryCount must be between {0} and {int.MaxValue}.");
        }

        /// <summary>
        /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
        /// don't validate the <see cref="ResilienceOptions.Retry"/> when the <see cref="ResilienceOptions.EnableRetryPolicy"/>.
        /// is false.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesOptionsValidation4()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddResiliencePolicies(options =>
                {
                    options.EnableRetryPolicy = false;
                    options.EnableCircuitBreakerPolicy = false;
                    options.EnableTimeoutPolicy = false;
                    options.Retry.RetryCount = -1; // this should cause validation to fail if the EnableRetryPolicy was set to true
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                });

            var serviceProvider = services.BuildServiceProvider();
            Should.NotThrow(() => serviceProvider.InstantiateNamedHttpClient(httpClientName));
        }
    }
}
