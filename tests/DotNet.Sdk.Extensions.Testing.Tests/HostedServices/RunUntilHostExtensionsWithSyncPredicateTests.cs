namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// For more info see <see cref="StartupHostedService"/> and <see cref="HostedServicesWebApplicationFactory"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilHostExtensionsWithSyncPredicateTests
{
#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose
    public static TheoryData<IHost, RunUntilPredicate, Type, string> ValidateArgumentsData =>
        new TheoryData<IHost, RunUntilPredicate, Type, string>
        {
            { null!, () => true, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'host')" },
            { CreateHost(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
        };
#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose

    private static IHost CreateHost()
    {
        return Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .Build();
    }

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate)"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsData))]
    public void ValidatesArguments(
        IHost? host,
        RunUntilPredicate predicate,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => host!.RunUntilAsync(predicate),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
        host?.Dispose();
    }

    public static TheoryData<IHost, RunUntilPredicate, Action<RunUntilOptions>, Type, string> ValidateArgumentsWithOptionsData =>
        new TheoryData<IHost, RunUntilPredicate, Action<RunUntilOptions>, Type, string>
        {
            { null!, () => true, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'host')" },
            { CreateHost(), null!, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
            { CreateHost(), () => true, null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" },
        };

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsWithOptionsData))]
    public void ValidatesArgumentsWithOptions(
        IHost? host,
        RunUntilPredicate predicate,
        Action<RunUntilOptions> configureOptions,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => host!.RunUntilAsync(predicate, configureOptions),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
        host?.Dispose();
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate)"/>
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

        // This code creating the Host would exist somewhere in app being tested.
        // In a real scenario we would call the function that creates the Host.
        // For this test we incorporate the host creation in this test.
        var hostBuilder = Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ICalculator, Calculator>();
                services.AddHostedService<MyBackgroundService>();
            });

        // This is for overriding services for test purposes.
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
            })
            .Build();

        await host.RunUntilAsync(() => callCount >= 3);
        callCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
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

        // This code creating the Host would exist somewhere in app being tested.
        // In a real scenario we would call the function that creates the Host.
        // For this test we incorporate the host creation in this test.
        var hostBuilder = Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ICalculator, Calculator>();
                services.AddHostedService<MyBackgroundService>();
            });

        // This is for overriding services for test purposes.
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
            })
            .Build();

        var runUntilTask = host.RunUntilAsync(() => callCount >= 4, options => options.Timeout = TimeSpan.FromSeconds(1));
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
    /// checks the predicate using the <seealso cref="RunUntilOptions.PredicateCheckInterval"/> value.
    /// This test sets up the PredicateCheckInterval and Timeout options values so that the timeout occurs even before the first check is made.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so if we set the timeout to 1s
    /// and the predicate to stop the Host after receiving at least 1 call then the timeout should NOT be triggered before the predicate is met.
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

        // This code creating the Host would exist somewhere in app being tested.
        // In a real scenario we would call the function that creates the Host.
        // For this test we incorporate the host creation in this test.
        var hostBuilder = Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ICalculator, Calculator>();
                services.AddHostedService<MyBackgroundService>();
            });

        // This is for overriding services for test purposes.
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
            })
            .Build();

        var runUntilTask = host.RunUntilAsync(() => callCount >= 1, options =>
        {
            options.PredicateCheckInterval = TimeSpan.FromSeconds(2);
            options.Timeout = TimeSpan.FromSeconds(1);
        });
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
        callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
