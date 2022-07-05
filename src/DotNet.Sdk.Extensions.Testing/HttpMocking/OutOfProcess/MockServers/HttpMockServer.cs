using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

internal abstract class HttpMockServer : IHttpMockServer
{
    private readonly HttpMockServerArgs _mockServerArgs;

    protected HttpMockServer(HttpMockServerArgs mockServerArgs)
    {
        _mockServerArgs = mockServerArgs ?? throw new ArgumentNullException(nameof(mockServerArgs));
    }

    public IHost? Host { get; private set; }

    public async Task<List<HttpMockServerUrl>> StartAsync()
    {
        if (Host is not null)
        {
            throw new InvalidOperationException($"The {nameof(HttpMockServer)} has already been started.");
        }

        Host = CreateHostBuilder(_mockServerArgs.HostArgs).Build();
        await Host.StartAsync();
        return Host
            .GetServerAddresses()
            .Select(x => x.ToHttpMockServerUrl())
            .ToList();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected abstract IHostBuilder CreateHostBuilder(string[] args);

    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Following guidance from https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync")]
    protected virtual async ValueTask DisposeAsyncCore()
    {
        Host?.StopAsync();
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
