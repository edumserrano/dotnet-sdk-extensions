using System.Diagnostics.CodeAnalysis;
using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.StartupValidation
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
    public class StartupMyOptions3ValidateEagerly
    {
        private readonly IConfiguration _configuration;

        public StartupMyOptions3ValidateEagerly(IConfiguration configuration)
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
                    return options.SomeOption > 1;
                })
                .ValidateEagerly();
        }

        public void Configure(IApplicationBuilder app)
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
