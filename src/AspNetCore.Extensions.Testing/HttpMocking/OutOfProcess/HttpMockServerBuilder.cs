using System;
using System.Collections.Generic;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess
{
    public class HttpMockServerBuilder
    {
        private string[] _hostArgs = new string[0];
        private readonly List<HttpMockServerUrlDescriptor> _hostUrls = new List<HttpMockServerUrlDescriptor>();

        public HttpMockServerBuilder UseUrl(HttpScheme scheme = HttpScheme.Http, int port = 0)
        {
            var urlDescriptor = new HttpMockServerUrlDescriptor(scheme, port);
            _hostUrls.Add(urlDescriptor);
            return this;
        }

        public HttpMockServerBuilder UseHostArgs(string[] hostArgs)
        {
            _hostArgs = hostArgs;
            return this;
        }

        public DefaultHttpMockServerBuilder UseHttpResponseMocks()
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new DefaultHttpMockServerBuilder(args);
        }

        public StartupBasedHttpMockServerBuilder<T> UseStartup<T>() where T : class
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new StartupBasedHttpMockServerBuilder<T>(args);
        }

    }

    public class DefaultHttpMockServerBuilder
    {
        private readonly HttpMockServerArgs _mockServerArgs;
        private readonly List<HttpResponseMock> _httpResponseMocks = new List<HttpResponseMock>();

        public DefaultHttpMockServerBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public DefaultHttpMockServerBuilder MockHttpResponse(HttpResponseMock httpResponseMock)
        {
            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public DefaultHttpMockServerBuilder MockHttpResponse(Action<HttpResponseMockBuilder> configureHttpResponseMock)
        {
            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            configureHttpResponseMock(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public HttpMockServer Build()
        {
            return new HttpMockServer(_mockServerArgs, _httpResponseMocks);
        }
    }

    public class StartupBasedHttpMockServerBuilder<T> where T : class
    {
        private readonly HttpMockServerArgs _mockServerArgs;

        public StartupBasedHttpMockServerBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public HttpMockServer<T> Build()
        {
            return new HttpMockServer<T>(_mockServerArgs);
        }
    }
}