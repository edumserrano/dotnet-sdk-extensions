namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
public class StartupHostedService
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICalculator, Calculator>();
        services.AddHostedService<MyBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app
            .UseWhen(_ => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
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
