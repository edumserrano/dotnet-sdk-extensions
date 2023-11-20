namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

internal sealed class StartupBasedHttpMockServer<T>(HttpMockServerArgs mockServerArgs) : HttpMockServer(mockServerArgs)
    where T : class
{
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
