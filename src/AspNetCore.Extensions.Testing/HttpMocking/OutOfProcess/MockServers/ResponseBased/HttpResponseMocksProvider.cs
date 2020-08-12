using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.ResponseMocking;

namespace AspNetCore.Extensions.Testing.HttpMocking.OutOfProcess.MockServers.ResponseBased
{
    public class HttpResponseMocksProvider
    {
        private readonly ICollection<HttpResponseMock> _httpResponseMocks;

        public HttpResponseMocksProvider(ICollection<HttpResponseMock> httpResponseMocks)
        {
            _httpResponseMocks = httpResponseMocks ?? throw new ArgumentNullException(nameof(httpResponseMocks));
        }

        public IEnumerable<HttpResponseMock> HttpResponseMocks => _httpResponseMocks.ToList();
    }
}