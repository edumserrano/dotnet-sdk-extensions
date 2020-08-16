using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    public class ResponseBasedBuilder
    {
        private readonly HttpMockServerArgs _mockServerArgs;
        private readonly List<HttpResponseMock> _httpResponseMocks = new List<HttpResponseMock>();

        internal ResponseBasedBuilder(HttpMockServerArgs args)
        {
            _mockServerArgs = args ?? throw new ArgumentNullException(nameof(args));
        }

        public ResponseBasedBuilder MockHttpResponse(HttpResponseMock httpResponseMock)
        {
            if (httpResponseMock == null) throw new ArgumentNullException(nameof(httpResponseMock));

            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public ResponseBasedBuilder MockHttpResponse(Action<HttpResponseMockBuilder> configureHttpResponseMock)
        {
            if (configureHttpResponseMock == null) throw new ArgumentNullException(nameof(configureHttpResponseMock));

            var httpResponseMockBuilder = new HttpResponseMockBuilder();
            configureHttpResponseMock(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public HttpMockServer Build()
        {
            return new ResponseBasedHttpMockServer(_mockServerArgs, _httpResponseMocks);
        }
    }
}