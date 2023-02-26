using System.Runtime.InteropServices;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

[StructLayout(LayoutKind.Auto)]
internal readonly struct HttpMockServerUrlDescriptor
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
        var scheme = Scheme.ToString().ToLowerInvariant();
        return $"{scheme}://*:{Port}";
    }
}
