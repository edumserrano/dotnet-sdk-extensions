using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers
{
    /// <summary>
    /// An implementation of <see cref="DelegatingHandler"/> that can be used for <see cref="HttpClient"/> testing.
    /// </summary>
    public class TestHttpMessageHandler : DelegatingHandler
    {
        private readonly List<HttpResponseMessageMock> _httpResponseMocks = new List<HttpResponseMessageMock>();

        /// <summary>
        /// Configure an <see cref="HttpResponseMessage"/> to be returned when executing an HTTP call via
        /// an <see cref="HttpClient"/> configured to use the <see cref="TestHttpMessageHandler"/>.
        /// </summary>
        /// <param name="configure">Action to configure the <see cref="HttpResponseMessage"/> to be returned.</param>
        /// <returns>The <see cref="TestHttpMessageHandler"/> for chaining.</returns>
        public TestHttpMessageHandler MockHttpResponse(Action<HttpResponseMessageMockBuilder> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            var httpResponseMockBuilder = new HttpResponseMessageMockBuilder();
            configure(httpResponseMockBuilder);
            var httpResponseMock = httpResponseMockBuilder.Build();
            MockHttpResponse(httpResponseMock);
            return this;
        }

        /// <summary>
        /// Configure an <see cref="HttpResponseMessage"/> to be returned when executing an HTTP call via
        /// an <see cref="HttpClient"/> configured to use the <see cref="TestHttpMessageHandler"/>.
        /// </summary>
        /// <param name="httpResponseMock">The <see cref="HttpResponseMessageMock"/> that defines the <see cref="HttpResponseMessage"/> that will be returned.</param>
        /// <returns>The <see cref="TestHttpMessageHandler"/> for chaining.</returns>
        public TestHttpMessageHandler MockHttpResponse(HttpResponseMessageMock httpResponseMock)
        {
            if (httpResponseMock is null) throw new ArgumentNullException(nameof(httpResponseMock));

            _httpResponseMocks.Add(httpResponseMock);
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
