namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

// For more information on why this custom WebApplicationFactory<T> is configured as below
// please see the doc at /docs/integration-tests/web-application-factory.md
public class HostedServicesWebApplicationFactory : WebApplicationFactory<StartupHostedService>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseContentRoot(".")
            .UseStartup<StartupHostedService>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureServices(services =>
            {
                services.IgnoreBackgroundServiceExceptions();
            })
            .ConfigureWebHostDefaults(_ =>
            {
            });
    }
}
