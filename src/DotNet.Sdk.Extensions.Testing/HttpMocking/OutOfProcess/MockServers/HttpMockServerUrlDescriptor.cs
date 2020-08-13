using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public readonly struct HttpMockServerUrlDescriptor
    {
        public HttpMockServerUrlDescriptor(HttpScheme scheme, int port)
        {
            Scheme = scheme;
            Port = port;
        }

        public HttpScheme Scheme { get; }

        public int Port { get; }

        public override string ToString()
        {
            var scheme = Scheme.ToString().ToLower();
            return $"{scheme}://*:{Port}";
        }
    }
}