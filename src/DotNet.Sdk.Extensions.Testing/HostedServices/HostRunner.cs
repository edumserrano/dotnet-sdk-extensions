namespace DotNet.Sdk.Extensions.Testing.HostedServices;

internal abstract class HostRunner : IAsyncDisposable
{
    public abstract Task StartAsync();

    public abstract Task StopAsync();

    public abstract ValueTask DisposeAsync();
}

internal sealed class DefaultHostRunner : HostRunner
{
    private readonly IHost _host;

    public DefaultHostRunner(IHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public override Task StartAsync()
    {
        return _host.StartAsync();
    }

    public override Task StopAsync()
    {
        return _host.StopAsync();
    }

    public override async ValueTask DisposeAsync()
    {
#if NET6_0 || NET7_0
        if (_host is WebApplication webApp)
        {
            await webApp.DisposeAsync();
            return;
        }

        _host.Dispose();
        await ValueTask.CompletedTask;
#else
        _host.Dispose();
        await Task.CompletedTask;
#endif
    }
}

internal sealed class WebApplicationFactoryHostRunner<T> : HostRunner
    where T : class
{
    private readonly WebApplicationFactory<T> _webApplicationFactory;

    public WebApplicationFactoryHostRunner(WebApplicationFactory<T> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory ?? throw new ArgumentNullException(nameof(webApplicationFactory));
    }

    public override Task StartAsync()
    {
        _ = _webApplicationFactory.Server;
        return Task.CompletedTask;
    }

    public override async Task StopAsync()
    {
        await DisposeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
#if NET6_0 || NET7_0
        await _webApplicationFactory.DisposeAsync();
#else
        _webApplicationFactory.Dispose();
        await Task.CompletedTask;
#endif
    }
}
