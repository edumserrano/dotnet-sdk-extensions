using System;
using System.Collections.Generic;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.StartupBased;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess
{
    public class HttpMockServerBuilder
    {
        public DefaultHttpMockServerBuilder UseHttpResponseMocks()
        {
            return new DefaultHttpMockServerBuilder();
        }

        public StartupBasedHttpMockServerBuilder<T> UseStartup<T>() where T : class
        {
            return new StartupBasedHttpMockServerBuilder<T>();
        }
    }

    public class DefaultHttpMockServerBuilder
    {
        private readonly List<HttpResponseMock> _httpResponseMocks = new List<HttpResponseMock>();

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
            return new HttpMockServer(_httpResponseMocks);
        }
    }

    public class StartupBasedHttpMockServerBuilder<T> where T : class
    {
        public HttpMockServer<T> Build()
        {
            return new HttpMockServer<T>();
        }
    }
}