using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public interface IHttpMockServerBuilder
    {
        IHttpMockServerBuilder UseHostArgs(string[] hostArgs);

        IResponseMockBasedHttpMockServerBuilder UseHttpResponseMocks();

        IStartupBasedHttpMockServerBuilder<T> UseStartup<T>() where T : class;

        IHttpMockServerBuilder UseUrl(HttpScheme scheme, int port);
    }

    public class HttpMockServerBuilder : IHttpMockServerBuilder
    {
        private string[] _hostArgs = new string[0];
        private readonly List<HttpMockServerUrlDescriptor> _hostUrls = new List<HttpMockServerUrlDescriptor>();

        public IHttpMockServerBuilder UseUrl(HttpScheme scheme, int port)
        {
            var urlDescriptor = new HttpMockServerUrlDescriptor(scheme, port);
            _hostUrls.Add(urlDescriptor);
            return this;
        }

        public IHttpMockServerBuilder UseHostArgs(string[] hostArgs)
        {
            _hostArgs = hostArgs ?? throw new ArgumentNullException(nameof(hostArgs));
            return this;
        }

        public IResponseMockBasedHttpMockServerBuilder UseHttpResponseMocks()
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new ResponseMockBasedHttpMockServerBuilder(args);
        }

        public IStartupBasedHttpMockServerBuilder<T> UseStartup<T>() where T : class
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new StartupBasedHttpMockServerBuilder<T>(args);
        }
    }
}