namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;

internal sealed class ResponseBasedHttpMockServer : HttpMockServer
{
    private readonly HttpResponseMocksProvider _httpResponseMocksProvider;

    public ResponseBasedHttpMockServer(HttpMockServerArgs mockServerArgs, IReadOnlyCollection<HttpResponseMock> httpResponseMocks)
        : base(mockServerArgs)
    {
        _httpResponseMocksProvider = new HttpResponseMocksProvider(httpResponseMocks);
    }

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
