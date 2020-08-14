using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    internal class HttpResponseMocksProvider
    {
        private readonly ICollection<IHttpResponseMock> _httpResponseMocks;

        public HttpResponseMocksProvider(ICollection<IHttpResponseMock> httpResponseMocks)
        {
            _httpResponseMocks = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
        }

        public IEnumerable<IHttpResponseMock> HttpResponseMocks => _httpResponseMocks.ToList();
    }
}