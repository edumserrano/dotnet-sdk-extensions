namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout;

// For more information on why this custom WebApplicationFactory<T> is configured as below
// please see the doc at /docs/integration-tests/web-application-factory.md
public class TimeoutHttpResponseMockingWebApplicationFactory : WebApplicationFactory<TimeoutStartupHttpResponseMocking>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseContentRoot(".")
            .UseStartup<TimeoutStartupHttpResponseMocking>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        /*
         * ConfigureWebHostDefaults is required to make sure all default web related services are
         * registered in the container
         */
        return Host.CreateDefaultBuilder()
            .UseDefaultLogLevel(LogLevel.Critical)
            .ConfigureWebHostDefaults(_ =>
            {
            });
    }
}
