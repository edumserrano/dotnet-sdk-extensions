namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;

internal sealed class ResponseBasedHttpMockServer(HttpMockServerArgs mockServerArgs, IReadOnlyCollection<HttpResponseMock> httpResponseMocks): HttpMockServer(mockServerArgs)
{
    private readonly HttpResponseMocksProvider _httpResponseMocksProvider = new HttpResponseMocksProvider(httpResponseMocks);

    protected override IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(_httpResponseMocksProvider);
                });
                webBuilder.UseStartup<HttpMockServerStartup>();
            });
    }
}
