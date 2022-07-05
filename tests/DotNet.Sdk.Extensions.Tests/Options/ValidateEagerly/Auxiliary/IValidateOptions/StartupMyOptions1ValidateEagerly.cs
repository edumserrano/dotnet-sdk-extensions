using System.Diagnostics.CodeAnalysis;
using DotNet.Sdk.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.IValidateOptions;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
public class StartupMyOptions1ValidateEagerly
{
    private readonly IConfiguration _configuration;

    public StartupMyOptions1ValidateEagerly(IConfiguration configuration)
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

    public void Configure(IApplicationBuilder app)
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
