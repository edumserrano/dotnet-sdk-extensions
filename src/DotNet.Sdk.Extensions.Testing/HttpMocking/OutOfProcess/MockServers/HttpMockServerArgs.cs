using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    internal class HttpMockServerArgs
    {
        // using the port 0 means that the app will randomly select a port (one for http and another for https) that aren't currently in use
        private const string _defaultUrls = "http://*:0;https://*:0";

        public HttpMockServerArgs(List<HttpMockServerUrlDescriptor> urlDescriptors, List<string> hostArgs)
        {
            if (hostArgs is null) throw new ArgumentNullException(nameof(hostArgs));
            HostArgs = CreateHostArgs(hostArgs, urlDescriptors);
        }

        public string[] HostArgs { get; }

        private static string[] CreateHostArgs(List<string> hostArgs, List<HttpMockServerUrlDescriptor> urlDescriptors)
        {
            if (hostArgs.Contains("--urls") && urlDescriptors.Any())
            {
                throw new InvalidOperationException($"Competing URLs configuration. URls defined via both {nameof(HttpMockServerBuilder)}.{nameof(HttpMockServerBuilder.UseUrl)} method and by defining an '--urls' arg via {nameof(HttpMockServerBuilder)}.{nameof(HttpMockServerBuilder.UseHostArgs)}. Use only one of these methods to configure the URLs.");
            }

            if (hostArgs.Contains("--urls"))
            {
                return hostArgs.ToArray();
            }

            // if the argument --urls wasn't given then make sure the URLs are defined
            var urls = BuildUrls(urlDescriptors);
            return hostArgs
                .Concat(new List<string> { "--urls", urls })
                .ToArray();
        }

        private static string BuildUrls(List<HttpMockServerUrlDescriptor> urlDescriptors)
        {
            if (urlDescriptors is null || !urlDescriptors.Any())
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
}
