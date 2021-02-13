using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.HostedServices.Auxiliary
{
    public class StartupHostedService
    {
        private readonly IConfiguration _configuration;

        public StartupHostedService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculator, Calculator>();
            services.AddHostedService<MyBackgroundService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.WriteAsync("hi from asp.net core app with background service");
                    });
                });
        }
    }
}
