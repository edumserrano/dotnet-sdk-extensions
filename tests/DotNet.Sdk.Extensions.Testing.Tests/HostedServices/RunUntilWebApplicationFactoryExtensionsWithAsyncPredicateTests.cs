namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// For more info see <see cref="StartupHostedService"/> and <see cref="HostedServicesWebApplicationFactory"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilWebApplicationFactoryExtensionsWithAsyncPredicateTests
{
#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose
    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Type, string> ValidateArgumentsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Type, string>
        {
            { null!, () => Task.FromResult(true), typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicateAsync')" },
        };
#pragma warning restore CA2000 // Dispose objects before losing scope - the test method will do the dispose

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync)"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsData))]
    public void ValidatesArguments(
        HostedServicesWebApplicationFactory? webApplicationFactory,
        RunUntilPredicateAsync predicateAsync,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => webApplicationFactory!.RunUntilAsync(predicateAsync),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
        webApplicationFactory?.Dispose();
    }

#pragma warning disable CA2000 // Dispose objects before losing scope - the test method will do the dispose
    public static TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Action<RunUntilOptions>, Type, string> ValidateArgumentsWithOptionsData =>
        new TheoryData<HostedServicesWebApplicationFactory, RunUntilPredicateAsync, Action<RunUntilOptions>, Type, string>
        {
            { null!, () => Task.FromResult(true), _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'webApplicationFactory')" },
            { new HostedServicesWebApplicationFactory(), null!, _ => { }, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'predicateAsync')" },
            { new HostedServicesWebApplicationFactory(), () => Task.FromResult(true), null!, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'configureOptions')" },
        };
#pragma warning restore CA2000 // Dispose objects before losing scope - the test method will do the dispose

    /// <summary>
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
    /// extension method.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidateArgumentsWithOptionsData))]
    public void ValidatesArgumentsWithOptions(
        HostedServicesWebApplicationFactory? webApplicationFactory,
        RunUntilPredicateAsync predicateAsync,
        Action<RunUntilOptions> configureOptions,
        Type exceptionType,
        string exceptionMessage)
    {
        var exception = Should.Throw(
            actual: () => webApplicationFactory!.RunUntilAsync(predicateAsync, configureOptions),
            exceptionType: exceptionType);
        exception.Message.ShouldBe(exceptionMessage);
        webApplicationFactory?.Dispose();
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync)"/>
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

        using var hostedServicesWebAppFactory = new HostedServicesWebApplicationFactory();
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
                });
            });

        await webApplicationFactory.RunUntilAsync(() => Task.FromResult(callCount >= 3));
        callCount.ShouldBeGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
    /// times out if the predicate is not met within the configured timeout.
    /// The <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 500 ms so if we set the timeout to 1s
    /// and the predicate to stop the Host after receiving 4 calls then the timeout should be triggered before the predicate is met.
    /// </summary>
    [Fact]
    public async Task TimeoutOption()
    {
        using var webApplicationFactory = new HostedServicesWebApplicationFactory();
        var runUntilTask = webApplicationFactory.RunUntilAsync(() => false /*run forever*/, options => options.Timeout = TimeSpan.FromSeconds(1));
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilAsync{T}(WebApplicationFactory{T},RunUntilPredicateAsync,Action{RunUntilOptions})"/>
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
                });
            });

        var runUntilTask = webApplicationFactory.RunUntilAsync(() => Task.FromResult(callCount >= 1), options =>
        {
            options.PredicateCheckInterval = TimeSpan.FromSeconds(2);
            options.Timeout = TimeSpan.FromSeconds(1);
        });
        var exception = await Should.ThrowAsync<RunUntilException>(runUntilTask);
        exception.Message.ShouldBe("RunUntilExtensions.RunUntilAsync timed out after 00:00:01. This means the Host was shutdown before the RunUntilExtensions.RunUntilAsync predicate returned true. If that's what you intended, meaning, if you want to run the Host for a set period of time, consider using RunUntilExtensions.RunUntilTimeoutAsync instead.");
        callCount.ShouldBeGreaterThanOrEqualTo(1); // this is true which means the RunUntilAsync predicate was met however it wasn't checked before the timeout was triggered
    }
}
