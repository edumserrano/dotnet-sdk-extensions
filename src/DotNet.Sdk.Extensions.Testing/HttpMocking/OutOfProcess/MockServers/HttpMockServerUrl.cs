using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    /// <summary>
    /// Represents an URL where the HTTP mock server is listening.
    /// </summary>
    public record HttpMockServerUrl
    {
        private readonly string _url;

        internal HttpMockServerUrl(HttpScheme scheme, string host, int port)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            _url = $"{scheme.ToString().ToLowerInvariant()}://{host}:{port}";
        }

        /// <summary>
        /// Gets the <see cref="HttpScheme"/> part of the URL.
        /// </summary>
        public HttpScheme Scheme { get; }

        /// <summary>
        /// Gets the host part of the URL.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets the port part of the URL.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Returns the string representation of the URL.
        /// </summary>
        /// <returns>The mock server's URL as a string.</returns>
        public override string ToString() => _url;

        /// <summary>
        /// Implicitly calls ToString().
        /// </summary>
        /// <param name="url">The <see cref="HttpMockServerUrl"/> to convert.</param>
        public static implicit operator string(HttpMockServerUrl url)
        {
            return url is null ? string.Empty : url.ToString();
        }
    }
}
