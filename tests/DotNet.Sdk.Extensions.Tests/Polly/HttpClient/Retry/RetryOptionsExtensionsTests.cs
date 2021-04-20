using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry
{
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
    }
}
