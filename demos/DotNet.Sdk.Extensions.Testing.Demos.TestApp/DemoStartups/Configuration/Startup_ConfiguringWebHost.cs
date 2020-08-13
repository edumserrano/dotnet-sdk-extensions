using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.TestApp.DemoStartups.Configuration
{
    public class Startup_ConfiguringWebHost
    {
        private readonly IConfiguration _configuration;

        public Startup_ConfiguringWebHost(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/message-one", async context =>
                    {
                        await context.Response.WriteAsync(_configuration["MessageOne"]);
                    });
                    endpoints.MapGet("/message-two", async context =>
                    {
                        await context.Response.WriteAsync(_configuration["MessageTwo"]);
                    });
                });
        }
    }
}
