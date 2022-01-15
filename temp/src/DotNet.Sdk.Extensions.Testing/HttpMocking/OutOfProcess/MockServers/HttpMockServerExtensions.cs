using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    internal static class HttpMockServerExtensions
    {
        public static ICollection<string> GetServerAddresses(this IHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var server = host.Services.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            return addressFeature is null
                ? new List<string>()
                : addressFeature.Addresses;
        }

        public static HttpMockServerUrl ToHttpMockServerUrl(this string address)
        {
            /* Example formats:
             * - http://[::]:56879
             * - https://127.0.0.1:56879
             * - https://[::]
             *
             */
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var split1 = address.Split("://");
            var httpScheme = Enum.Parse<HttpScheme>(split1[0], ignoreCase: true);
            var split2 = split1[1]
                .Replace("[::]", "localhost", StringComparison.OrdinalIgnoreCase)
                .Split(":");
            var host = split2[0];
            var port = split2.Length > 1
                ? int.Parse(split2[1], CultureInfo.InvariantCulture)
                : httpScheme == HttpScheme.Http
                    ? 80
                    : 443;
            return new HttpMockServerUrl(httpScheme, host, port);
        }
    }
}
