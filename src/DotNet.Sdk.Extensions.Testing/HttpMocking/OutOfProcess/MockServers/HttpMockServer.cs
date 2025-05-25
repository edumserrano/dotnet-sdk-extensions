namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

internal abstract class HttpMockServer : IHttpMockServer
{
    private readonly HttpMockServerArgs _mockServerArgs;

    protected HttpMockServer(HttpMockServerArgs mockServerArgs)
    {
        _mockServerArgs = mockServerArgs ?? throw new ArgumentNullException(nameof(mockServerArgs));
    }

    public IHost? Host { get; private set; }

    public async Task<IList<HttpMockServerUrl>> StartAsync()
    {
        if (Host is not null)
        {
            throw new InvalidOperationException($"The {nameof(HttpMockServer)} has already been started.");
        }

        Host = CreateHostBuilder(_mockServerArgs.HostArgs).Build();
        await Host.StartAsync();
        var httpMockServerUrls = Host
            .GetServerAddresses()
            .Select(x => x.ToHttpMockServerUrl());
        return [.. httpMockServerUrls];
    }

    // Implemented following guidance from https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected abstract IHostBuilder CreateHostBuilder(string[] args);

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (Host is not null)
        {
            await Host.StopAsync();
        }

        switch (Host)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
            default:
                break;
        }
    }
}
