using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public interface IStartupBasedHttpMockServerBuilder<T> where T : class
    {
        IHttpMockServer Build();
    }

    internal class StartupBasedHttpMockServerBuilder<T> : IStartupBasedHttpMockServerBuilder<T> where T : class
    {
        private readonly HttpMockServerArgs _mockServerArgs;

        public StartupBasedHttpMockServerBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IHttpMockServer Build()
        {
            return new HttpMockServer<T>(_mockServerArgs);
        }
    }
}