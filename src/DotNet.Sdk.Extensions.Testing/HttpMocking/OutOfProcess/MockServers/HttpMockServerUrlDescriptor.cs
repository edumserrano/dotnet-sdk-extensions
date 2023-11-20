using System.Runtime.InteropServices;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

[StructLayout(LayoutKind.Auto)]
internal readonly struct HttpMockServerUrlDescriptor(HttpScheme scheme, int port)
{
    public HttpScheme Scheme { get; } = scheme;

    public int Port { get; } = port;

    public override string ToString()
    {
        var scheme = Scheme.ToString().ToLowerInvariant();
        return $"{scheme}://*:{Port}";
    }
}
