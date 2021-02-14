using DotNet.Sdk.Extensions.Testing.Tests.Auxiliary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.UseHttpMocks
{
    // For more information on why this custom WebApplicationFactory<T> is configured as below
    // please see the doc at /docs/integration-tests/web-application-factory.md 
    public class HttpResponseMockingWebApplicationFactory : WebApplicationFactory<StartupHttpResponseMocking>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(".")
                .UseStartup<StartupHttpResponseMocking>();
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