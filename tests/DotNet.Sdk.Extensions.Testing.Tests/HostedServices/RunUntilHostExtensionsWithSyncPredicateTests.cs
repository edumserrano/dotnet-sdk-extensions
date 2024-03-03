namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilHostExtensionsWithSyncPredicateTests
{
    public static TheoryData<IHost, RunUntilPredicate, Type, string> ValidateArgumentsData =>
        new TheoryData<IHost, RunUntilPredicate, Type, string>
        {
            { null!, () => true, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'host')" },
            { CreateHost(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
        };

    private static IHost CreateHost()
    {
        return Host
            .CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .Build();
    }

    /// <summary>
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate)"/>
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
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
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
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate)"/>
    /// terminates the Host after the predicate is met.
    /// The <see cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> which means that the predicate condition should be met
    /// before the default <see cref="RunUntilOptions.Timeout"/>.
    /// </summary>
    [Fact(Timeout = 3000)]
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
        var testScheduler = new TestScheduler();
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
                services.AddSingleton<IScheduler>(testScheduler);
            })
            .Build();

        var runUntilTask = host.RunUntilAsync(() => callCount == 3, _ => { }, testScheduler);
        callCount.ShouldBe(0);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(1);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(2);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(3);
        await runUntilTask;
        callCount.ShouldBe(3);
    }

    /// <summary>
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
    /// times out if the predicate is not met within the configured timeout.
    /// The <see cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> and this test sets the predicate so that it won't be met
    /// before the timeout occurs.
    /// </summary>
    [Fact(Timeout = 3000)]
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
        var testScheduler = new TestScheduler();
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
                services.AddSingleton<IScheduler>(testScheduler);
            })
            .Build();

        var timeout = TimeSpan.FromMilliseconds(50);
        var runUntilTask = host.RunUntilAsync(
                predicate: () => callCount >= 1,
                configureOptions: options =>
                {
                    options.Timeout = timeout;
                },
                scheduler: testScheduler);
        testScheduler.AdvanceBy(timeout.Ticks);

        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:00.0500000. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
    }

    /// <summary>
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync(IHost,RunUntilPredicate,Action{RunUntilOptions})"/>
    /// checks the predicate using the <see cref="RunUntilOptions.PredicateCheckInterval"/> value.
    /// The <see cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> and this test sets the predicate so that it SHOULD be triggered
    /// before the timeout occurs. However, the timeout is indeed triggered before the predicate is met because this
    /// test sets up the PredicateCheckInterval and Timeout options values so that the timeout occurs even before the
    /// first predicate check is made.
    /// </summary>
    [Fact(Timeout = 3000)]
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
                services.AddSingleton<IScheduler>(DefaultScheduler.Instance);
                services.AddHostedService<MyBackgroundService>();
            });

        // This is for overriding services for test purposes.
        var testScheduler = new TestScheduler();
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(calculator);
                services.AddSingleton<IScheduler>(testScheduler);
            })
            .Build();

        var timeoutMargin = MyBackgroundService.Period / 2;
        var timeout = (MyBackgroundService.Period * 2) + timeoutMargin;
        var runUntilTask = host.RunUntilAsync(
            predicate: () => callCount >= 1,
            configureOptions: options =>
            {
                options.PredicateCheckInterval = timeout * 2;
                options.Timeout = timeout;
            },
            scheduler: testScheduler);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        testScheduler.AdvanceBy(timeoutMargin.Ticks);

        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:00.2500000. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
        callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
