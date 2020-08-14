using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public interface IResponseMockBasedHttpMockServerBuilder
    {
        IHttpMockServer Build();

        IResponseMockBasedHttpMockServerBuilder MockHttpResponse(Action<IHttpResponseMockBuilder> configureHttpResponseMock);

        IResponseMockBasedHttpMockServerBuilder MockHttpResponse(IHttpResponseMock httpResponseMock);
    }

    internal class ResponseMockBasedHttpMockServerBuilder : IResponseMockBasedHttpMockServerBuilder
    {
        private readonly HttpMockServerArgs _mockServerArgs;
        private readonly List<IHttpResponseMock> _httpResponseMocks = new List<IHttpResponseMock>();

        public ResponseMockBasedHttpMockServerBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IResponseMockBasedHttpMockServerBuilder MockHttpResponse(IHttpResponseMock httpResponseMock)
        {
            if (httpResponseMock == null) throw new ArgumentNullException(nameof(httpResponseMock));

            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public IResponseMockBasedHttpMockServerBuilder MockHttpResponse(Action<IHttpResponseMockBuilder> configureHttpResponseMock)
        {
            if (configureHttpResponseMock == null) throw new ArgumentNullException(nameof(configureHttpResponseMock));

            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            configureHttpResponseMock(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public IHttpMockServer Build()
        {
            return new HttpMockServer(_mockServerArgs, _httpResponseMocks);
        }
    }
}