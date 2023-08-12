namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Extensions;

/// <summary>
/// Tests for the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions"/> class.
/// Specifically for the <see cref="CircuitBreakerOptions"/> validation.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddCircuitBreakerPolicyOptionsValidationTests
{
    /// <summary>
    /// Tests that the CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods
    /// validate the <see cref="CircuitBreakerOptions"/> with the built in data annotations.
    ///
    /// Validates that the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> needs to be a double > 0.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2.2)]
    [InlineData(-3.5)]
    public void AddCircuitBreakerPolicyOptionsValidationForDurationOfBreakInSecs(double durationOfBreakInSecs)
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(options =>
            {
                options.DurationOfBreakInSecs = durationOfBreakInSecs;
                options.SamplingDurationInSecs = 60;
                options.FailureThreshold = 0.6;
                options.MinimumThroughput = 10;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"DataAnnotation validation failed for 'CircuitBreakerOptions' members: 'DurationOfBreakInSecs' with the error: 'The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
    }

    /// <summary>
    /// Tests that the CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods
    /// validate the <see cref="CircuitBreakerOptions"/> with the built in data annotations.
    ///
    /// Validates that the <see cref="CircuitBreakerOptions.SamplingDurationInSecs"/> needs to be a double > 0.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2.2)]
    [InlineData(-3.5)]
    public void AddCircuitBreakerPolicyOptionsValidationForSamplingDurationInSecs(double samplingDurationInSecs)
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(options =>
            {
                options.DurationOfBreakInSecs = 1;
                options.SamplingDurationInSecs = samplingDurationInSecs;
                options.FailureThreshold = 0.6;
                options.MinimumThroughput = 10;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"DataAnnotation validation failed for 'CircuitBreakerOptions' members: 'SamplingDurationInSecs' with the error: 'The field SamplingDurationInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
    }

    /// <summary>
    /// Tests that the CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods
    /// validate the <see cref="CircuitBreakerOptions"/> with the built in data annotations.
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
    public void AddCircuitBreakerPolicyOptionsValidationForFailureThreshold(double failureThreshold)
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(options =>
            {
                options.DurationOfBreakInSecs = 1;
                options.SamplingDurationInSecs = 60;
                options.FailureThreshold = failureThreshold;
                options.MinimumThroughput = 10;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"DataAnnotation validation failed for 'CircuitBreakerOptions' members: 'FailureThreshold' with the error: 'The field FailureThreshold must be between {double.Epsilon} and {1}.'.");
    }

    /// <summary>
    /// Tests that the CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods
    /// validate the <see cref="CircuitBreakerOptions"/> with the built in data annotations.
    ///
    /// Validates that the <see cref="CircuitBreakerOptions.MinimumThroughput"/> needs to be an int >= 2.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddCircuitBreakerPolicyOptionsValidationForMinimumThroughput(int minimumThroughput)
    {
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(options =>
            {
                options.DurationOfBreakInSecs = 1;
                options.SamplingDurationInSecs = 60;
                options.FailureThreshold = 0.6;
                options.MinimumThroughput = minimumThroughput;
            });

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"DataAnnotation validation failed for 'CircuitBreakerOptions' members: 'MinimumThroughput' with the error: 'The field MinimumThroughput must be between {2} and {int.MaxValue}.'.");
    }

    /// <summary>
    /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
    /// after using <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> and those
    /// option configurations will be honored.
    ///
    /// In this test we configure the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> to 1 and
    /// force a validation that this value must be > 3.
    /// Although the default data annotation validations only enforces that the value must be >= 0, with the
    /// extra validation the options validation will fail.
    /// </summary>
    [Fact]
    public void AddCircuitBreakerPolicyOptionsValidation2()
    {
        const string httpClientName = "GitHub";
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientCircuitBreakerOptions(optionsName)
            .Configure(options =>
            {
                options.DurationOfBreakInSecs = 1;
                options.SamplingDurationInSecs = 60;
                options.FailureThreshold = 0.6;
                options.MinimumThroughput = 10;
            })
            .Validate(options =>
            {
                return options.DurationOfBreakInSecs > 3;
            });
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(optionsName);

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe("A validation error has occurred.");
    }

    /// <summary>
    /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
    /// after using <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> and
    /// those option configurations will be honored.
    ///
    /// In this test we configure the <see cref="CircuitBreakerOptions.DurationOfBreakInSecs"/> to -1 and
    /// force a validation that this value must be > 3.
    /// With this setup both the default data annotation validation and the custom one will be triggered.
    /// </summary>
    [Fact]
    public void AddCircuitBreakerPolicyOptionsValidation3()
    {
        const string httpClientName = "GitHub";
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientCircuitBreakerOptions(optionsName)
            .Configure(options =>
            {
                options.DurationOfBreakInSecs = -1;
                options.SamplingDurationInSecs = 60;
                options.FailureThreshold = 0.6;
                options.MinimumThroughput = 10;
            })
            .Validate(options =>
            {
                return options.DurationOfBreakInSecs > 3;
            });
        services
            .AddHttpClient(httpClientName)
            .AddCircuitBreakerPolicy(optionsName);

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
        });
        exception.Message.ShouldBe($"A validation error has occurred.; DataAnnotation validation failed for 'CircuitBreakerOptions' members: 'DurationOfBreakInSecs' with the error: 'The field DurationOfBreakInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
    }
}
