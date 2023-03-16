namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices;

/// <summary>
/// These tests simulate an app with a <see cref="BackgroundService"/>.
/// For more info see <see cref="StartupHostedService"/> and <see cref="HostedServicesWebApplicationFactory"/>.
/// </summary>
[Trait("Category", XUnitCategories.HostedServices)]
public class RunUntilTimeoutTests : IClassFixture<HostedServicesWebApplicationFactory>
{
    private readonly HostedServicesWebApplicationFactory _hostedServicesWebAppFactory;

    public RunUntilTimeoutTests(HostedServicesWebApplicationFactory hostedServicesWebApplicationFactory)
    {
        _hostedServicesWebAppFactory = hostedServicesWebApplicationFactory;
    }

    /// <summary>
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
    /// extension method.
    /// </summary>
    [Fact]
    public async Task WebApplicationFactoryRunUntilTimeoutValidatesArguments()
    {
        var webApplicationFactoryArgumentNullException = await Should.ThrowAsync<ArgumentNullException>(() =>
        {
            return RunUntilExtensions.RunUntilTimeoutAsync<StartupHostedService>(webApplicationFactory: null!, TimeSpan.FromSeconds(1));
        });
        webApplicationFactoryArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'webApplicationFactory')");
    }

    /// <summary>
    /// Validates the arguments for the <see cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
    /// extension method.
    /// </summary>
    [Fact]
    public async Task HostRunUntilTimeoutValidatesArguments()
    {
        var hostArgumentNullException = await Should.ThrowAsync<ArgumentNullException>(() =>
        {
            return RunUntilExtensions.RunUntilTimeoutAsync(host: null!, TimeSpan.FromSeconds(1));
        });
        hostArgumentNullException.Message.ShouldBe("Value cannot be null. (Parameter 'host')");
    }

    /// <summary>
    /// Tests that <see cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
    /// terminates the Host created by the WebApplicationFactory after the specified timeout.
    /// The <see cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> which means that the when the host is terminated there should be
    /// 3 calls to that method.
    /// </summary>
    [Fact(Timeout = 3000)]
    public async Task WebApplicationFactoryRunUntilTimeout()
    {
        var callCount = 0;
        var calculator = Substitute.For<ICalculator>();
        calculator
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(_ =>
            {
                ++callCount;
            });

        var testScheduler = new TestScheduler();
#if NET6_0 || NET7_0
        await using var hostedServicesWebAppFactory = _hostedServicesWebAppFactory
#else
        using var hostedServicesWebAppFactory = _hostedServicesWebAppFactory
#endif
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(calculator);
                    services.AddSingleton<IScheduler>(testScheduler);
                });
            });

        var runUntilTimeout = TimeSpan.FromMilliseconds(MyBackgroundService.Period.TotalMilliseconds * 3);
        var runUntilTimeoutTask = hostedServicesWebAppFactory.RunUntilTimeoutAsync(runUntilTimeout, testScheduler);
        callCount.ShouldBe(0);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(1);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(2);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(3);
        await runUntilTimeoutTask;
        callCount.ShouldBe(3);
    }

    /// <summary>
    /// Tests that <see cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
    /// terminates the Host after the specified timeout.
    /// The <see cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every
    /// <see cref="MyBackgroundService.Period"/> which means that the when the host is terminated there should be
    /// 3 calls to that method.
    /// </summary>
    [Fact(Timeout = 3000)]
    public async Task HostRunUntilTimeout()
    {
        var callCount = 0;
        var calculator = Substitute.For<ICalculator>();
        calculator
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(_ =>
            {
                ++callCount;
            });

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
                services.AddSingleton<IScheduler>(DefaultScheduler.Instance); // required because I'm using RX on MyBackgroundService to timetravel
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

        var runUntilTimeout = TimeSpan.FromMilliseconds(MyBackgroundService.Period.TotalMilliseconds * 3);
        var runUntilTimeoutTask = host.RunUntilTimeoutAsync(runUntilTimeout, testScheduler);
        callCount.ShouldBe(0);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(1);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(2);
        testScheduler.AdvanceBy(MyBackgroundService.Period.Ticks);
        callCount.ShouldBe(3);
        await runUntilTimeoutTask;
        callCount.ShouldBe(3);
    }
}
