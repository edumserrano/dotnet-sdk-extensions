using Microsoft.Reactive.Testing;

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
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate)"/>
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
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> which means that the predicate condition should be met
    /// before the default <seealso cref="RunUntilOptions.Timeout"/>.
    /// Furthermore I'm using the <see cref="TestScheduler"/> to control the passing of time on the
    /// <seealso cref="MyBackgroundService"/>. This allows me to make the test more deterministic.
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

        var testScheduler = new TestScheduler();
        var hostedServicesWebAppFactory = new HostedServicesWebApplicationFactory();
#if NET6_0 || NET7_0
        await using var webApplicationFactory = hostedServicesWebAppFactory
#else
        using var webApplicationFactory = hostedServicesWebAppFactory
#endif
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount == 3, options => options.PredicateCheckInterval = TimeSpan.FromMilliseconds(5));
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        await runUntilTask;

        callCount.ShouldBe(3);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
    /// times out if the predicate is not met within the configured timeout.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> and this test sets the predicate so that it won't be met
    /// before the timeout occurs.
    /// Furthermore I'm using the <see cref="TestScheduler"/> to control the passing of time on the
    /// <seealso cref="MyBackgroundService"/>. This allows me to make the test more deterministic.
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

        var testScheduler = new TestScheduler();
        using var hostedServicesWebApplicationFactory = new HostedServicesWebApplicationFactory();
#if NET6_0 || NET7_0
        await using var webApplicationFactory = hostedServicesWebApplicationFactory
#else
        using var webApplicationFactory = hostedServicesWebApplicationFactory
#endif
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount >= 1, options => options.Timeout = TimeSpan.FromMilliseconds(50));
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:00.0500000. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicate,Action{RunUntilOptions})"/>
    /// checks the predicate using the <seealso cref="RunUntilOptions.PredicateCheckInterval"/> value.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> and this test sets the predicate so that it SHOULD be triggered
    /// before the timeout occurs. However, the timeout is indeed triggered before the predicate is met because this
    /// test sets up the PredicateCheckInterval and Timeout options values so that the timeout occurs even before the
    /// first predicate check is made.
    /// Furthermore I'm using the <see cref="TestScheduler"/> to control the passing of time on the
    /// <seealso cref="MyBackgroundService"/>. This allows me to make the test more deterministic.
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

        var testScheduler = new TestScheduler();
        using var hostedServicesWebApplicationFactory = new HostedServicesWebApplicationFactory();
#if NET6_0 || NET7_0
        await using var webApplicationFactory = hostedServicesWebApplicationFactory
#else
        using var webApplicationFactory = hostedServicesWebApplicationFactory
#endif
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => callCount >= 1, options =>
        {
            options.PredicateCheckInterval = TimeSpan.FromSeconds(3);
            options.Timeout = TimeSpan.FromSeconds(1);
        });
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);

        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
        callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
