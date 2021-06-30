using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.StartupValidation
{
    public class StartupMyOptions3ValidateEargerly
    {
        private readonly IConfiguration _configuration;

        public StartupMyOptions3ValidateEargerly(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions<MyOptions3>()
                .Bind(_configuration)
                .Validate(options =>
                {
                    if (options.SomeOption > 1)
                    {
                        return true;
                    }
                    return false;
                })
                .ValidateEagerly();
        }

        public static void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        var myOptions = context.RequestServices.GetRequiredService<MyOptions3>();
                        await context.Response.WriteAsync($"{myOptions.SomeOption}");
                    });
                });
        }
    }
}
