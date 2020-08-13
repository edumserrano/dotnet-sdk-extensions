using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Demos.Options.OptionsValue
{
    public class Startup_OptionsValue
    {
        private readonly IConfiguration _configuration;

        public Startup_OptionsValue(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISomeClass, SomeClass>();
            services
                .AddOptions<MyOptions>(_configuration, sectionName: "MyOptionsSection")
                .AddOptionsValue();
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
                        var someClass = context.RequestServices.GetRequiredService<ISomeClass>();
                        var message = someClass.GetMessage();
                        await context.Response.WriteAsync(message);
                    });
                });
        }
    }
}
