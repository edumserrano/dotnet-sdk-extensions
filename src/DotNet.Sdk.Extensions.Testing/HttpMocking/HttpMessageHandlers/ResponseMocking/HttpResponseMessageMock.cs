using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Represents a mock of an <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class HttpResponseMessageMock
    {
        private readonly HttpResponseMessageMockPredicateDelegate _predicate;
        private readonly HttpResponseMessageMockHandlerDelegate _handler;

        internal HttpResponseMessageMock(
            HttpResponseMessageMockPredicateDelegate predicate,
            HttpResponseMessageMockHandlerDelegate handler)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        internal async Task<HttpResponseMessageMockResult> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
