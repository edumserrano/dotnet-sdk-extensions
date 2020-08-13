using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public class HttpMockServerArgs
    {
        private readonly ICollection<HttpMockServerUrlDescriptor> _urlDescriptors;

        public HttpMockServerArgs(List<HttpMockServerUrlDescriptor> urlDescriptors, string[] hostArgs)
        {
            if (urlDescriptors is null || !urlDescriptors.Any())
            {
                throw new HttpMockServerException($"You need to configure at least one host URL: use {nameof(HttpMockServerBuilder)}.{nameof(HttpMockServerBuilder.UseUrl)} method to configure mock server URLs.");
            }

            _urlDescriptors = urlDescriptors ?? throw new ArgumentNullException(nameof(urlDescriptors));
            var urls = BuildUrls(urlDescriptors);
            HostArgs = hostArgs
                .Concat(new List<string> { "--urls", urls })
                .ToArray();
        }

        public string[] HostArgs { get; }

        public IEnumerable<HttpMockServerUrlDescriptor> HostUrls => _urlDescriptors.ToList();

        private string BuildUrls(List<HttpMockServerUrlDescriptor> urlDescriptors)
        {
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
