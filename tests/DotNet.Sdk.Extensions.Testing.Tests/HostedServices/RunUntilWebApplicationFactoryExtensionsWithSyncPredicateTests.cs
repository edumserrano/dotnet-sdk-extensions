namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// For more info see <see cref="StartupHostedService"/> and <see cref="HostedServicesWebApplicationFactory"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilWebApplicationFactoryExtensionsWithSyncPredicateTests
{
#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose
    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Type, string> ValidateArgumentsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Type, string>
        {
            { null!, () => true, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
        };
#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose

    /// <summary>
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate)"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsData))]
    public void ValidatesArguments(
        HostedServicesWebApplicationFactory? webApplicationFactory,
        RunUntilPredicate predicate,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => webApplicationFactory!.RunUntilAsync(predicate),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
        webApplicationFactory?.Dispose();
    }

    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Action<RunUntilOptions>, Type, string> ValidateArgumentsWithOptionsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicate, Action<RunUntilOptions>, Type, string>
        {
            { null!, () => true, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicate')" },
            { new HostedServicesWebApplicationFactory(), () => true, null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" },
        };

    /// <summary>
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
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
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate)"/>
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

        var testScheduler = new TestScheduler();
        await using var hostedServicesWebAppFactory = new HostedServicesWebApplicationFactory();
        await using var webApplicationFactory = hostedServicesWebAppFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount == 3, _ => { }, testScheduler);
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
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
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

        var testScheduler = new TestScheduler();
        await using var hostedServicesWebApplicationFactory = new HostedServicesWebApplicationFactory();
        await using var webApplicationFactory = hostedServicesWebApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var timeout = TimeSpan.FromMilliseconds(50);
        var runUntilTask = webApplicationFactory.RunUntilAsync(
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
    /// Tests that <see cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
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

        var testScheduler = new TestScheduler();
        await using var hostedServicesWebApplicationFactory = new HostedServicesWebApplicationFactory();
        await using var webApplicationFactory = hostedServicesWebApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var timeoutMargin = MyBackgroundService.Period / 2;
        var timeout = (MyBackgroundService.Period * 2) + timeoutMargin;
        var runUntilTask = webApplicationFactory.RunUntilAsync(
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
        callCount.ShouldBe(2); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
