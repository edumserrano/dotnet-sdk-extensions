namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

internal sealed class StartupBasedHttpMockServer<T> : HttpMockServer
    where T : class
{
    public StartupBasedHttpMockServer(HttpMockServerArgs mockServerArgs)
        : base(mockServerArgs)
    {
    }

    protected override IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<T>();
            });
    }
}
