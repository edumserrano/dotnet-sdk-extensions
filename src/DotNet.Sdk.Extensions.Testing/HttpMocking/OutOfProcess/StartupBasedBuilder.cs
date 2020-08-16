using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public class StartupBasedBuilder<T> where T : class
    {
        private readonly HttpMockServerArgs _mockServerArgs;

        internal StartupBasedBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public HttpMockServer Build()
        {
            return new StartupBasedHttpMockServer<T>(_mockServerArgs);
        }
    }
}