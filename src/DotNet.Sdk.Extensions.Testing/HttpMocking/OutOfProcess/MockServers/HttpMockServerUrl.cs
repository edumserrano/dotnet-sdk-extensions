using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public readonly struct HttpMockServerUrl
    {
        private readonly string _url;

        public HttpMockServerUrl(HttpScheme scheme, string host, int port)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            _url = $"{scheme.ToString().ToLower()}://{host}:{port}";
        }

        public HttpScheme Scheme { get; }

        public string Host { get; }

        public int Port { get; }

        public override string ToString() => _url;

        public static implicit operator string(HttpMockServerUrl url) => url.ToString();
    }
}