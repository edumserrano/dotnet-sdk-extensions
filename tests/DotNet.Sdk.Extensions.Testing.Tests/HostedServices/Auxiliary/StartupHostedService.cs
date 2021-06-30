using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary
{
    public class StartupHostedService
    {
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddSingleton<ICalculator, Calculator>();
            _ = services.AddHostedService<MyBackgroundService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    _ = endpoints.MapGet("/", async context =>
                      {
                          await context.Response.WriteAsync("hi from asp.net core app with background service");
                      });
                });
        }
    }
}
