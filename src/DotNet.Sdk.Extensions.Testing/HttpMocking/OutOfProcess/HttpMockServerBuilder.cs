using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public class HttpMockServerBuilder
    {
        private string[] _hostArgs = new string[0];
        private readonly List<HttpMockServerUrlDescriptor> _hostUrls = new List<HttpMockServerUrlDescriptor>();

        public HttpMockServerBuilder UseUrl(HttpScheme scheme, int port)
        {
            var urlDescriptor = new HttpMockServerUrlDescriptor(scheme, port);
            _hostUrls.Add(urlDescriptor);
            return this;
        }

        public HttpMockServerBuilder UseHostArgs(string[] hostArgs)
        {
            _hostArgs = hostArgs ?? throw new ArgumentNullException(nameof(hostArgs));
            return this;
        }

        public ResponseBasedBuilder UseHttpResponseMocks()
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new ResponseBasedBuilder(args);
        }

        public StartupBasedBuilder<T> UseStartup<T>() where T : class
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new StartupBasedBuilder<T>(args);
        }
    }
}