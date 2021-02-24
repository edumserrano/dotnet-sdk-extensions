using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration.Auxiliary
{
    public class StartupOverrideConfiguration
    {
        private readonly IConfiguration _configuration;

        public StartupOverrideConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //assuming the app had an appsettings.json that could be binded against the MyOptions type
            services.AddOptions<MyOptions>().Bind(_configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/options", async context =>
                    {
                        await context.Response.WriteAsync(_configuration["Option1"]);
                    });
                });
        }
    }

    public class MyOptions
    {
        public string Option1 { get; set; }
    }
}
