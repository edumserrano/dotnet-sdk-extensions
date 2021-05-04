using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically for the <see cref="RetryOptions"/> validation.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddRetryPolicyOptionsValidationTests
    {
        /// <summary>
        /// Tests that the RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods
        /// validate the <see cref="RetryOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.RetryCount"/> needs to be an int >= 0.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-3)]
        public void AddRetryPolicyOptionsValidationForRetryCount(int retryCount)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = 1;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'RetryCount' with the error: 'The field RetryCount must be between {0} and {int.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that the RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods
        /// validate the <see cref="RetryOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.RetryCount"/> can be zero.
        /// </summary>
        [Fact]
        public void AddRetryPolicyOptionsValidationForRetryCount2()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 0;
                    options.MedianFirstRetryDelayInSecs = 1;
                });

            var serviceProvider = services.BuildServiceProvider();
            Should.NotThrow(() => serviceProvider.InstantiateNamedHttpClient(httpClientName));
        }

        /// <summary>
        /// Tests that the RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods
        /// validate the <see cref="RetryOptions"/> with the built in data annotations.
        ///
        /// Validates that the <see cref="RetryOptions.MedianFirstRetryDelayInSecs"/> needs to be a double > 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        [InlineData(-3.5)]
        public void AddRetryPolicyOptionsValidationForMedianFirstRetryDelayInSecs(int medianFirstRetryDelayInSecs)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 1;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'MedianFirstRetryDelayInSecs' with the error: 'The field MedianFirstRetryDelayInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.RetryCount"/> to 2 and force a validation
        /// that this value must be > 3.
        /// Although the default data annotation validations only enforces that the value must be >= 0, with the
        /// extra validation the options validation will fail.
        /// </summary>
        [Fact]
        public void AddRetryPolicyOptionsValidation2()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 2;
                    options.MedianFirstRetryDelayInSecs = 1;
                })
                .Validate(options =>
                {
                    return options.RetryCount > 3;
                });
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.RetryCount"/> to -1 and force a validation
        /// that this value must be > 3.
        /// With this setup both the default data annotation validation and the custom one will be triggered.
        /// </summary>
        [Fact]
        public void AddRetryPolicyOptionsValidation3()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = -1;
                    options.MedianFirstRetryDelayInSecs = 1;
                })
                .Validate(options =>
                {
                    return options.RetryCount > 3;
                })
                ;
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"A validation error has occurred.; DataAnnotation validation failed for members: 'RetryCount' with the error: 'The field RetryCount must be between {0} and {int.MaxValue}.'.");
        }
    }
}
