using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Represents a mock of an <see cref="HttpResponseMessage"/>
    /// </summary>
    public class HttpResponseMessageMock
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

        /// <summary>
        /// Executes the <see cref="HttpResponseMessage"/> mock.
        /// </summary>
        /// <param name="request"> The <see cref="HttpRequestMessage"/> passed to the <see cref="HttpResponseMessage"/> mock execution.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> passed to the <see cref="HttpResponseMessage"/> mock execution.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
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