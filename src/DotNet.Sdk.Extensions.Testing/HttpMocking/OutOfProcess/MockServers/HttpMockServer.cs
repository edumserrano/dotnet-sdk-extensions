using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    /// <summary>
    /// HTTP server to be used for tests when you want to mock out of process HTTP responses.
    /// </summary>
    public abstract class HttpMockServer : IAsyncDisposable
    {
        private readonly HttpMockServerArgs _mockServerArgs;
        private IHost? _host;

        internal HttpMockServer(HttpMockServerArgs mockServerArgs)
        {
            _mockServerArgs = mockServerArgs ?? throw new ArgumentNullException(nameof(mockServerArgs));
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>The URLs where the server is listening for requests.</returns>
        public async Task<List<HttpMockServerUrl>> StartAsync()
        {
            if (_host != null)
            {
                throw new InvalidOperationException($"The {nameof(HttpMockServer)} has already been started.");
            }

            _host = CreateHostBuilder(_mockServerArgs.HostArgs).Build();
            await _host.StartAsync();
            return _host
                .GetServerAddresses()
                .Select(x => x.ToHttpMockServerUrl())
                .ToList();
        }

        protected abstract IHostBuilder CreateHostBuilder(string[] args);

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