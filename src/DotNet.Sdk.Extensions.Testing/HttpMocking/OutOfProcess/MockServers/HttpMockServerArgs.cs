using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public class HttpMockServerArgs
    {
        private const string _defaultUrls = "http://*:0;https://*:0";

        public HttpMockServerArgs(List<HttpMockServerUrlDescriptor> urlDescriptors, string[] hostArgs)
        {
            if (hostArgs == null) throw new ArgumentNullException(nameof(hostArgs));

            var urls = BuildUrls(urlDescriptors);
            HostArgs = hostArgs
                .Concat(new List<string> { "--urls", urls })
                .ToArray();
        }

        public string[] HostArgs { get; }

        private string BuildUrls(List<HttpMockServerUrlDescriptor> urlDescriptors)
        {
            if (urlDescriptors is null || !urlDescriptors.Any())
            {
                return _defaultUrls;
            }

            var sb = new StringBuilder();
            foreach (var url in urlDescriptors.Select(x => x.ToString()))
            {
                sb.Append(url);
                sb.Append(";");
            }

            return sb.ToString();
        }
    }
}
