using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.IValidateOptions
{
    public class StartupMyOptions1ValidateEargerly
    {
        private readonly IConfiguration _configuration;

        public StartupMyOptions1ValidateEargerly(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IValidateOptions<MyOptions1>, MyOptions1Validation>()
                .AddOptions<MyOptions1>()
                .Bind(_configuration)
                .ValidateEagerly();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        var myOptions = context.RequestServices.GetRequiredService<MyOptions1>();
                        await context.Response.WriteAsync(myOptions.SomeOption);
                    });
                });
        }
    }
}
