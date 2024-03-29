namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;

internal sealed class HttpMockServerArgs
{
    // using the port 0 means that the app will randomly select a port (one for http and another for https) that aren't currently in use
    private const string _defaultUrls = "http://*:0;https://*:0";

    public HttpMockServerArgs(List<HttpMockServerUrlDescriptor> urlDescriptors, List<string> hostArgs)
    {
        ArgumentNullException.ThrowIfNull(hostArgs);

        HostArgs = CreateHostArgs(hostArgs, urlDescriptors);
    }

    public string[] HostArgs { get; }

    private static string[] CreateHostArgs(List<string> hostArgs, List<HttpMockServerUrlDescriptor> urlDescriptors)
    {
        if (hostArgs.Contains("--urls", StringComparer.InvariantCulture) && urlDescriptors.Count > 0)
        {
            throw new InvalidOperationException($"Competing URLs configuration. URls defined via both {nameof(HttpMockServerBuilder)}.{nameof(HttpMockServerBuilder.UseUrls)} method and by defining an '--urls' arg via {nameof(HttpMockServerBuilder)}.{nameof(HttpMockServerBuilder.UseHostArgs)}. Use only one of these methods to configure the URLs.");
        }

        if (hostArgs.Contains("--urls", StringComparer.InvariantCulture))
        {
            return [.. hostArgs];
        }

        // if the argument --urls wasn't given then make sure the URLs are defined
        var urls = BuildUrls(urlDescriptors);
        return [.. hostArgs, .. new List<string> { "--urls", urls }];
    }

    private static string BuildUrls(List<HttpMockServerUrlDescriptor> urlDescriptors)
    {
        if (urlDescriptors.Count == 0)
        {
            return _defaultUrls;
        }

        var sb = new StringBuilder();
        foreach (var url in urlDescriptors.Select(x => x.ToString()))
        {
            sb.Append(url);
            sb.Append(';');
        }

        return sb.ToString();
    }
}
