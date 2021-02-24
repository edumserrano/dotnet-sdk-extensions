using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration.Auxiliary
{
    // For more information on why this custom WebApplicationFactory<T> is configured as below
    // please see the doc at /docs/integration-tests/web-application-factory.md 
    // You usually do NOT need to create a custom class that implements WebApplicationFactory
    // We require this because there are multiple Startup classes in this project
    public class UseDefaultLogLevelWebApplicationFactory : WebApplicationFactory<StartupUseDefaultLogLevel>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(".")
                .UseStartup<StartupUseDefaultLogLevel>();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {

                });
        }
    }
}