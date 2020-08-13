using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers
{
    public static class HttpMockServerExtensions
    {
        public static ICollection<string> GetServerAddresses(this IHost host)
        {
            var server = host.Services.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            return addressFeature.Addresses;
        }

        public static HttpMockServerUrl ToHttpMockServerUrl(this string address)
        {
            /* Example formats:
             * - http://[::]:56879
             * - https://127.0.0.1:56879
             * - https://[::]
             *
             */
            var split1 = address.Split("://");
            var schemeAsEnum = Enum.Parse<HttpScheme>(split1[0], ignoreCase: true);
            var split2 = split1[1].Replace("[::]","localhost").Split(":");
            var host = split2[0];
            var port = split2.Length > 1 
                ? int.Parse(split2[1]) 
                : schemeAsEnum  == HttpScheme.Http 
                    ? 80 
                    : 443;
            return new HttpMockServerUrl(schemeAsEnum, host, port);
        }
    }
}