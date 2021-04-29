using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout
{
    /// <summary>
    /// Tests for the <see cref="TimeoutOptionsExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class TimeoutOptionsExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> extension method
        /// adds to the <see cref="ServiceCollection"/> an <see cref="IOptions{TOptions}"/>
        /// where TOptions is of type <see cref="TimeoutOptions"/>.
        ///
        /// It also checks that the <see cref="TimeoutOptions"/> has the expected values.
        /// </summary>
        [Fact]
        public void AddHttpClientTimeoutOptions()
        {
            var optionsName = "timeoutOptions";
            var timeoutInSecs = 3;
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            var serviceProvider = services.BuildServiceProvider();
            var timeoutOptions = serviceProvider.GetHttpClientTimeoutOptions(optionsName);
            timeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> extension method
        /// validates the <see cref="TimeoutOptions"/>. Can only be positive value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2.2)]
        public void AddHttpClientTimeoutOptionsValidatesOptions(double timeoutInSecs)
        {
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                });
            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                return serviceProvider.GetHttpClientTimeoutOptions(optionsName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }
    }
}
