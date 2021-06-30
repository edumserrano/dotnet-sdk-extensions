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

        internal HttpMockServer(HttpMockServerArgs mockServerArgs)
        {
            _mockServerArgs = mockServerArgs ?? throw new ArgumentNullException(nameof(mockServerArgs));
        }

        /// <summary>
        /// The <see cref="IHost"/> used by the <see cref="HttpMockServer"/>
        /// </summary>
        public IHost? Host { get; private set; }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>The URLs where the server is listening for requests.</returns>
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

        /// <summary>
        /// Creates the <see cref="IHostBuilder"/> used by the <see cref="HttpMockServer"/> to create the <see cref="Host"/> used.
        /// </summary>
        /// <param name="args">The arguments passed in to the <see cref="IHostBuilder"/>.</param>
        /// <returns>An instance of <see cref="IHostBuilder"/>.</returns>
        protected abstract IHostBuilder CreateHostBuilder(string[] args);

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
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
}
