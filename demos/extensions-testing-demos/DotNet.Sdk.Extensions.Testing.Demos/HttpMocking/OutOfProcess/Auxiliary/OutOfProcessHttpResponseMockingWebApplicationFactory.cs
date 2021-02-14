using DotNet.Sdk.Extensions.Testing.Demos.Auxiliary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Demos.HttpMocking.OutOfProcess.Auxiliary
{
    // For more information on why this custom WebApplicationFactory<T> is configured as below
    // please see the doc at /docs/integration-tests/web-application-factory.md 
    // You might NOT need to create a custom class that implements WebApplicationFactory
    // We require this because there are multiple Startup classes in this project
    public class OutOfProcessHttpResponseMockingWebApplicationFactory : WebApplicationFactory<OutOfProcessHttpResponseMockingStartup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(".")
                .UseStartup<OutOfProcessHttpResponseMockingStartup>();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            /*
             * ConfigureWebHostDefaults is required to make sure all default web related services are
             * registered in the container
             */
            return Host.CreateDefaultBuilder()
                .SetDefaultLogLevel(LogLevel.Critical)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                });
        }
    }
}