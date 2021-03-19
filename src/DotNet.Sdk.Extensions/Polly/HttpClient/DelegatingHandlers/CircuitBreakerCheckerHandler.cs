using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.DelegatingHandlers
{
    internal class CircuitBreakerCheckerHandler : DelegatingHandler
    {
        private readonly ICircuitBreakerPolicy _circuitBreaker;

        public CircuitBreakerCheckerHandler(ICircuitBreakerPolicy circuitBreaker)
        {
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            // no point in trying to make the request because the circuit breaker will throw an exception.
            // avoid exception as indicated by https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken
            if (_circuitBreaker.IsCircuitOpen())
            {
                return new CircuitBrokenHttpResponseMessage();
            }

            return await base.SendAsync(httpRequestMessage, cancellationToken);
        }
    }
}
