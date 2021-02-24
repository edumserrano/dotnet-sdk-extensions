using DotNet.Sdk.Extensions.Testing.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary
{
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
                .SetDefaultLogLevel(LogLevel.Critical)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                });
        }
    }
}