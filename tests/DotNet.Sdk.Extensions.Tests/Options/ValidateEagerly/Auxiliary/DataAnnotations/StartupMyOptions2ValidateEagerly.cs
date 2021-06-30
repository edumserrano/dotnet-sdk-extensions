using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.DataAnnotations
{
    public class StartupMyOptions2ValidateEagerly
    {
        private readonly IConfiguration _configuration;

        public StartupMyOptions2ValidateEagerly(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _ = services
                .AddOptions<MyOptions2>()
                .Bind(_configuration)
                .ValidateDataAnnotations()
                .ValidateEagerly();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    _ = endpoints.MapGet("/", async context =>
                      {
                          var myOptions = context.RequestServices.GetRequiredService<MyOptions2>();
                          await context.Response.WriteAsync(myOptions.SomeOption);
                      });
                });
        }
    }
}
