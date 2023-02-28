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
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
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
    /// Validates the arguments for the <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
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
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync{T}(WebApplicationFactory{T},TimeSpan)"/>
    /// terminates the Host created by the WebApplicationFactory after the specified timeout.
    /// Furthermore the <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 1s so
    /// we should also have 3 calls to that method.
    /// </summary>
    [Fact]
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

        var sw = Stopwatch.StartNew();
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
                });
            });
        await hostedServicesWebAppFactory.RunUntilTimeoutAsync(TimeSpan.FromMilliseconds(3300));
        sw.Stop();

        sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(3000));
        callCount.ShouldBe(3);
    }

    /// <summary>
    /// Tests that <seealso cref="RunUntilExtensions.RunUntilTimeoutAsync(IHost,TimeSpan)"/>
    /// terminates the Host after the specified timeout.
    /// Furthermore the <seealso cref="MyBackgroundService"/> BackgroundService calls ICalculator.Sum once every 1s so
    /// we should also have 3 calls to that method.
    /// </summary>
    [Fact]
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
            });

        // This is for overriding services for test purposes.
        using var host = hostBuilder
            .ConfigureServices((_, services) =>
             {
                 services.AddSingleton(calculator);
             })
            .Build();

        var sw = Stopwatch.StartNew();
        await host.RunUntilTimeoutAsync(TimeSpan.FromMilliseconds(3300));
        sw.Stop();

        sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(3000));
        callCount.ShouldBe(3);
    }
}
