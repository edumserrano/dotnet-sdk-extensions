namespace DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.DataAnnotations;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
public class StartupMyOptions2ValidateEagerly
{
    private readonly IConfiguration _configuration;

    public StartupMyOptions2ValidateEagerly(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddOptions<MyOptions2>()
            .Bind(_configuration)
            .ValidateDataAnnotations()
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
                    var myOptions = context.RequestServices.GetRequiredService<MyOptions2>();
                    await context.Response.WriteAsync(myOptions.SomeOption, context.RequestAborted);
                });
            });
    }
}
