using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public interface IStartupBasedBuilder<T> where T : class
    {
        IHttpMockServer Build();
    }

    internal class StartupBasedBuilder<T> : IStartupBasedBuilder<T> where T : class
    {
        private readonly HttpMockServerArgs _mockServerArgs;

        public StartupBasedBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IHttpMockServer Build()
        {
            return new HttpMockServer<T>(_mockServerArgs);
        }
    }
}