namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// For more info see <see cref="StartupHostedService"/> and <see cref="HostedServicesWebApplicationFactory"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilWebApplicationFactoryExtensionsWithSyncPredicateTests
{
    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Type, string> ValidateArgumentsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Type, string>
        {
            { null!, () => true, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
        };

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate)"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsData))]
    public void ValidatesArguments(
        HostedServicesWebApplicationFactory webApplicationFactory,
        RunUntilPredicate predicate,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => webApplicationFactory.RunUntilAsync(predicate),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
    }

    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Action<RunUntilOptions>, Type, string> ValidateArgumentsWithOptionsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Action<RunUntilOptions>, Type, string>
        {
            { null!, () => true, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
            { new HostedServicesWebApplicationFactory(), () => true, null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" },
        };

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsWithOptionsData))]
    public void ValidatesArgumentsWithOptions(
        HostedServicesWebApplicationFactory webApplicationFactory,
        RunUntilPredicate predicate,
        Action<RunUntilOptions> configureOptions,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => webApplicationFactory.RunUntilAsync(predicate, configureOptions),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate)"/>
    /// terminates the Host after the predicate is met.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms and the default
    /// <seealso cref="RunUntilOptions.Timeout"/> is 5 seconds so the predicate should be met before the timeout.
    /// </summary>
    [Fact]
    public async Task RunUntil()
    {
        var callCount = 0;
        var calculator = Substitute.For<ICalculator>();
        calculator
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(_ => ++callCount);

        var webApplicationFactory = new HostedServicesWebApplicationFactory();
        await webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                });
            })
            .RunUntilAsync(() => callCount >= 3);

        callCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
    /// times out if the predicate is not met within the configured timeout.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so if we set the timeout to 1s
    /// and the predicate to stop the Host after receiving at least 4 calls then the timeout should be triggered before the predicate is met.
    /// </summary>
    [Fact]
    public async Task TimeoutOption()
    {
        var callCount = 0;
        var calculator = Substitute.For<ICalculator>();
        calculator
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(_ => ++callCount);

        var webApplicationFactory = new HostedServicesWebApplicationFactory()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount >= 4, options => options.Timeout = TimeSpan.FromSeconds(1));
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
    /// checks the predicate using the <seealso cref="RunUntilOptions.PredicateCheckInterval"/> value.
    /// This test sets up the PredicateCheckInterval and Timeout options values so that the timeout occurs even before the first check is made.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so if we set the timeout to 1s
    /// and the predicate to stop the Host after receiving 1 call then the timeout should NOT be triggered before the predicate is met.
    /// However, the timeout is indeed triggered before the predicate is met because this test sets up the PredicateCheckInterval and Timeout options values
    /// so that the timeout occurs even before the first check is made.
    /// </summary>
    [Fact]
    public async Task PredicateCheckIntervalOption()
    {
        var callCount = 0;
        var calculator = Substitute.For<ICalculator>();
        calculator
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(_ => ++callCount);

        var webApplicationFactory = new HostedServicesWebApplicationFactory()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount >= 1, options =>
        {
            options.PredicateCheckInterval = TimeSpan.FromSeconds(2);
            options.Timeout = TimeSpan.FromSeconds(1);
        });
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
        callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
