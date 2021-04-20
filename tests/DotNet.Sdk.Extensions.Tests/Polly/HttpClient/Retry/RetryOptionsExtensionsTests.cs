using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry
{
    /// <summary>
    /// Tests for the <see cref="RetryOptionsExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class RetryOptionsExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="AddHttpClientRetryOptions"/> extension method
        /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
        /// where TOptions is of type <see cref="RetryOptions"/>.
        ///
        /// It also checks that the <see cref="RetryOptions"/> has the expected values.
        /// </summary>
        [Fact]
        public void AddHttpClientRetryOptions()
        {
            var optionsName = "retryOptions";
            var retryCount = 3;
            var medianFirstRetryDelayInSecs = 1;
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

        /// <summary>
        /// Tests that the <see cref="AddHttpClientRetryOptions"/> extension method
        /// validates the <see cref="RetryOptions.MedianFirstRetryDelayInSecs"/>.
        /// Can only be a positive number.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientRetryOptionsValidatesOptions(double medianFirstRetryDelayInSecs)
        {
            var optionsName = "retryOptions";
            var retryCount = 3;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientRetryOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'MedianFirstRetryDelayInSecs' with the error: 'The field MedianFirstRetryDelayInSecs must be between 5E-324 and 1.7976931348623157E+308.'.");
        }
        
        /// <summary>
        /// Tests that the <see cref="AddHttpClientRetryOptions"/> extension method
        /// validates the <see cref="RetryOptions.RetryCount"/>. Needs to be a number >= 0.
        /// 
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void AddHttpClientRetryOptionsValidatesOptions2(int retryCount)
        {
            var optionsName = "retryOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientRetryOptions(optionsName);
            });
            exception.Message.ShouldBe("DataAnnotation validation failed for members: 'RetryCount' with the error: 'The field RetryCount must be between 0 and 2147483647.'.");
        }

        /// <summary>
        /// Tests that the <see cref="AddHttpClientRetryOptions"/> extension method
        /// validates the <see cref="RetryOptions.RetryCount"/> can be zero.
        /// </summary>
        [Fact]
        public void AddHttpClientRetryOptionsValidatesOptions3()
        {
            var optionsName = "retryOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 0;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider
                .GetHttpClientRetryOptions(optionsName)
                .ShouldNotBeNull();
        }
    }
}
