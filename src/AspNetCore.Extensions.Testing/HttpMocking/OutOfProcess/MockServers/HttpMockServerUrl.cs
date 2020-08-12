using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public readonly struct HttpMockServerUrl
    {
        public HttpMockServerUrl(HttpScheme scheme, int port)
        {
            Scheme = scheme;
            Port = port;
        }

        public HttpScheme Scheme { get; }

        public int Port { get; }

        public override string ToString()
        {
            var scheme = Scheme.ToString().ToLower();
            return $"{scheme}://localhost:{Port}";
        }

        public static implicit operator string(HttpMockServerUrl url)
        {
            return url.ToString();
        }
    }
}