using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TimeoutPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically for the <see cref="TimeoutOptions"/> validation.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddTimeoutPolicyOptionsValidationTests
    {
        /// <summary>
        /// Tests that the TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy methods
        /// validate the <see cref="TimeoutOptions"/> with the built in data annotations.
        /// 
        /// Validates that the <see cref="TimeoutOptions.TimeoutInSecs"/> needs to be a double > 0.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddTimeoutPolicyOptionsValidation1(int timeoutInSecs)
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                });

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="TimeoutOptions.TimeoutInSecs"/> to 1 and force a validation
        /// that this value must be > 3.
        /// Although the default data annotation validations only enforces that the value must be positive, with the
        /// extra validation the options validation will fail.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyOptionsValidation2()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1)
                .Validate(options =>
                {
                    return options.TimeoutInSecs > 3;
                });
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName);

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="TimeoutOptions.TimeoutInSecs"/> to -1 and force a validation
        /// that this value must be > 3.
        /// With this setup both the default data annotation validation and the custom one will be triggered.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyOptionsValidation3()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = -1)
                .Validate(options =>
                {
                    return options.TimeoutInSecs > 3;
                });
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName);

            using var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"A validation error has occurred.; DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }
    }
}
