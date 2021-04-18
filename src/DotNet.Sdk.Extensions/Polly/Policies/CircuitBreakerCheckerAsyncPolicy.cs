using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Policies
{
    public class CircuitBreakerCheckerAsyncPolicy<T> : AsyncPolicy<T>
    {
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly Func<Context, CancellationToken, Task<T>> _factory;

        // factory method following Polly's guidelines for custom policies: 
        // http://www.thepollyproject.org/2019/02/13/authoring-a-proactive-polly-policy-custom-policies-part-ii/
        public static CircuitBreakerCheckerAsyncPolicy<T> Create(
            ICircuitBreakerPolicy circuitBreakerPolicy,
            Func<Context, CancellationToken, Task<T>> factory)
        {
            return new CircuitBreakerCheckerAsyncPolicy<T>(circuitBreakerPolicy, factory);
        }

        private CircuitBreakerCheckerAsyncPolicy(ICircuitBreakerPolicy circuitBreakerPolicy, Func<Context, CancellationToken, Task<T>> factory)
        {
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
            _factory = factory;
        }

        protected override async Task<T> ImplementationAsync(
            Func<Context, CancellationToken, Task<T>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // No point in trying to make the request because the circuit breaker will throw an exception.
            // Avoid exception as indicated by https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken
            if (_circuitBreakerPolicy.IsCircuitOpen())
            {
                var result = await _factory(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                return result;
            }

            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
