using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public abstract class HttpMockServerBase : IAsyncDisposable
    {
        private IHost? _host;

        public async Task Start(string[] args)
        {
            _host = CreateHostBuilder(args).Build();
            await _host.StartAsync();
        }

        public abstract IHostBuilder CreateHostBuilder(string[] args);

        public async ValueTask DisposeAsync()
        {
            _host?.StopAsync();
            switch (_host)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}