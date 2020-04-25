using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Extensions.Testing.HttpMocking
{
    internal class TestHttpMessageHandler : DelegatingHandler
    {
        private readonly List<HttpResponseMock> _httpResponseMocks;

        public TestHttpMessageHandler()
        {
            _httpResponseMocks = new List<HttpResponseMock>();
        }

        public void MockHttpResponse(HttpResponseMock httpResponseMock)
        {
            _httpResponseMocks.Add(httpResponseMock);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var httpResponseMock in _httpResponseMocks)
            {
                var responseMockResult = await  httpResponseMock.ExecuteAsync(request);
                if (responseMockResult.Status == HttpResponseMockResults.Executed)
                {
                    return responseMockResult.HttpResponseMessage;
                }
            }
            throw new InvalidOperationException($"No response mock defined for {request.Method} to {request.RequestUri}.");
        }
    }
}
