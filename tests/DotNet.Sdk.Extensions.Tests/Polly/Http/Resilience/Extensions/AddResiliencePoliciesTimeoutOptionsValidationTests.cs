using System.ComponentModel.DataAnnotations;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions;

/// <summary>
/// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
/// Specifically for the <see cref="ResilienceOptions.Timeout"/> validation.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddResiliencePoliciesTimeoutOptionsValidationTests
{
    /// <summary>
    /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
    /// validate the <see cref="ResilienceOptions"/> with the built in data annotations.
    ///
    /// Validates that the <see cref="ResilienceOptions.Timeout"/> cannot be null.
    /// </summary>
    [Fact]
    public void AddResiliencePoliciesOptionsValidationForRetryOptions()
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddResiliencePolicies(options =>
            {
                options.Timeout = null!;
                options.EnableRetryPolicy = false;
                options.EnableCircuitBreakerPolicy = false;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<ValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe("The Timeout field is required.");
    }

    /// <summary>
    /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
    /// validate the <see cref="ResilienceOptions.Timeout"/> with the built in data annotations.
    ///
    /// Validates that the <see cref="TimeoutOptions.TimeoutInSecs"/> needs to be a double > 0.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2.2)]
    public void AddResiliencePoliciesOptionsValidation1(int timeoutInSecs)
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddResiliencePolicies(options =>
            {
                options.Timeout.TimeoutInSecs = timeoutInSecs;
                options.EnableRetryPolicy = false;
                options.EnableCircuitBreakerPolicy = false;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<ValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.");
    }

    /// <summary>
    /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
    /// after using <see cref="ResilienceOptionsExtensions.AddHttpClientResilienceOptions"/> and those option configurations
    /// will be honored.
    ///
    /// In this test we configure the <see cref="TimeoutOptions.TimeoutInSecs"/> to 1 and force a validation
    /// that this value must be > 3.
    /// Although the default data annotation validations only enforces that the value must be positive, with the
    /// extra validation the options validation will fail.
    /// </summary>
    [Fact]
    public void AddResiliencePoliciesOptionsValidation2()
    {
        const string httpClientName = "GitHub";
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientResilienceOptions(optionsName)
            .Configure(options =>
            {
                options.Timeout.TimeoutInSecs = 1;
                options.EnableRetryPolicy = false;
                options.EnableCircuitBreakerPolicy = false;
            })
            .Validate(options =>
            {
                return options.Timeout.TimeoutInSecs > 3;
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
    /// In this test we configure the <see cref="TimeoutOptions.TimeoutInSecs"/> to -1 and force a validation
    /// that this value must be > 3.
    /// With this setup both the default data annotation validation and the custom one will be triggered.
    /// </summary>
    [Fact]
    public void AddResiliencePoliciesOptionsValidation3()
    {
        const string httpClientName = "GitHub";
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientResilienceOptions(optionsName)
            .Configure(options =>
            {
                options.EnableRetryPolicy = false;
                options.EnableCircuitBreakerPolicy = false;
                options.Timeout.TimeoutInSecs = -1;
            })
            .Validate(options =>
            {
                return options.Timeout.TimeoutInSecs > 3;
            });
        services
            .AddHttpClient(httpClientName)
            .AddResiliencePolicies(optionsName);

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<ValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.");
    }

    /// <summary>
    /// Tests that the ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies methods
    /// don't validate the <see cref="ResilienceOptions.Timeout"/> when the <see cref="ResilienceOptions.EnableTimeoutPolicy"/>.
    /// is false.
    /// </summary>
    [Fact]
    public void AddResiliencePoliciesOptionsValidation4()
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddResiliencePolicies(options =>
            {
                options.EnableRetryPolicy = false;
                options.EnableCircuitBreakerPolicy = false;
                options.EnableTimeoutPolicy = false;
                options.Timeout.TimeoutInSecs = -1; // this should cause validation to fail if the EnableTimeoutPolicy was set to true
            });

        var serviceProvider = services.BuildServiceProvider();
        Should.NotThrow(() => serviceProvider.InstantiateNamedHttpClient(httpClientName));
    }
}
