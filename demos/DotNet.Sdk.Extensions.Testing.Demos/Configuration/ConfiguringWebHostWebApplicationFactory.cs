using DotNet.Sdk.Extensions.Testing.Demos.TestApp.DemoStartups.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    public class ConfiguringWebHostWebApplicationFactory : WebApplicationFactory<StartupConfiguringWebHost>
    {
        /*
         * This is required because when using WebApplicationFactory<T>
         * it doesn't mean that the T type is going to be used as the Startup class. The startup will be discovered
         * via convention by scanning the assembly where the T type resides.
         * Since I have multiple Startup classes on the assembly for StartupConfiguringWebHost I want to make sure that
         * the one that is used is the StartupConfiguringWebHost.
         *
         * If you don't have multiple Startup classes in a single assembly your tests don't necessarily
         * require a custom WebApplicationFactory and can just use the WebApplicationFactory<T>
         */
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<StartupConfiguringWebHost>();
        }
    }
}