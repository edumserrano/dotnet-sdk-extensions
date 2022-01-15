using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    internal abstract class HostRunner : IDisposable
    {
        public abstract Task StartAsync();

        public abstract Task StopAsync();

        public abstract void Dispose();
    }

    internal class DefaultHostRunner : HostRunner
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

        public override void Dispose()
        {
            _host.Dispose();
        }
    }

    internal class WebApplicationFactoryHostRunner<T> : HostRunner
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

        public override Task StopAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
    }
}
