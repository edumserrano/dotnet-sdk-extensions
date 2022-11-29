namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Ignore for Startup type classes. Used as generic type param.")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup type classes.")]
internal sealed class HttpMockServerStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<DefaultResponseMiddleware>()
            .AddSingleton<ResponseMocksMiddleware>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app
            .UseResponseMocks()
            .RunDefaultResponse();
    }
}
