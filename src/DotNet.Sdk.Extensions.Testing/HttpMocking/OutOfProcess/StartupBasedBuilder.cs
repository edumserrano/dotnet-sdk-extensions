using System;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    /// <summary>
    /// Provides methods to configure and create an <see cref="HttpMockServer"/> when based on a Startup class.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the Startup class.</typeparam>
    public class StartupBasedBuilder<T>
        where T : class
    {
        private readonly HttpMockServerArgs _mockServerArgs;

        internal StartupBasedBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        /// <summary>
        /// Creates an <see cref="HttpMockServer"/> instance.
        /// </summary>
        /// <returns>The <see cref="HttpMockServer"/> instance.</returns>
        public IHttpMockServer Build()
        {
            return new StartupBasedHttpMockServer<T>(_mockServerArgs);
        }
    }
}
