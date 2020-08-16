using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    internal class HttpResponseMessageMock : IHttpResponseMessageMock
    {
        private readonly HttpResponseMessageMockPredicateDelegate _predicate;
        private readonly HttpResponseMessageMockHandlerDelegate _handler;

        public HttpResponseMessageMock(
            HttpResponseMessageMockPredicateDelegate predicate,
            HttpResponseMessageMockHandlerDelegate handler)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public async Task<IHttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var shouldExecute = await _predicate(request, cancellationToken);
            if (!shouldExecute)
            {
                return HttpResponseMessageMockResult.Skipped();
            }
            var httpResponseMessage = await _handler(request, cancellationToken);
            return HttpResponseMessageMockResult.Executed(httpResponseMessage);
        }
    }
}