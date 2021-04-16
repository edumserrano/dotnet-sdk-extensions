using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly
{
    internal class CircuitBreakerCheckerAsyncPolicy : AsyncPolicy<HttpResponseMessage>
    {
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;

        public CircuitBreakerCheckerAsyncPolicy(ICircuitBreakerPolicy circuitBreakerPolicy)
        {
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
        }

        protected override async Task<HttpResponseMessage> ImplementationAsync(
            Func<Context, CancellationToken, Task<HttpResponseMessage>> action,
            Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // no point in trying to make the request because the circuit breaker will throw an exception.
            // avoid exception as indicated by https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken
            if (_circuitBreakerPolicy.IsCircuitOpen())
            {
                return new CircuitBrokenHttpResponseMessage();
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
