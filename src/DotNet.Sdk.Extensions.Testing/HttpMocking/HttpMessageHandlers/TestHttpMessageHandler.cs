using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    public class TestHttpMessageHandler : DelegatingHandler
    {
        private readonly List<IHttpResponseMessageMock> _httpResponseMocks;

        public TestHttpMessageHandler()
        {
            _httpResponseMocks = new List<IHttpResponseMessageMock>();
        }

        public TestHttpMessageHandler MockHttpResponse(IHttpResponseMessageMock httpResponseMock)
        {
            if (httpResponseMock == null) throw new ArgumentNullException(nameof(httpResponseMock));

            _httpResponseMocks.Add(httpResponseMock);
            return this;
        }

        public TestHttpMessageHandler MockHttpResponse(Action<IHttpResponseMessageMockBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var httpResponseMockBuilder = new HttpResponseMessageMockBuilder();
            configure(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            MockHttpResponse(httpResponseMock);
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var httpResponseMock in _httpResponseMocks)
            {
                var responseMockResult = await httpResponseMock.ExecuteAsync(request, cancellationToken);
                if (responseMockResult.Status == HttpResponseMessageMockResults.Executed)
                {
                    return responseMockResult.HttpResponseMessage;
                }
            }
            throw new InvalidOperationException($"No response mock defined for {request.Method} to {request.RequestUri}.");
        }
    }
}
