using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Policies
{
    /// <summary>
    /// The purpose of this class is just to provide a slightly different syntax usage of
    /// the <see cref="CircuitBreakerCheckerAsyncPolicy{T}.Create"/> factory method.
    /// </summary>
    /// <remarks>
    /// It allows keeping the same syntax between extension and non-extension methods because
    /// instead of doing <see cref="CircuitBreakerCheckerAsyncPolicy{T}.Create"/> it allows doing
    /// <see cref="Create{T}"/>
    /// </remarks>
    public static class CircuitBreakerCheckerAsyncPolicy
    {
        /// <summary>
        /// Create a policy to to check if a circuit breaker is opened and avoid throwing an exception if the circuit is open/isolated. 
        /// </summary>
        /// <remarks>
        /// If the state of the circuit breaker policy is open or isolated then the policy chain execution is short circuited and the
        /// factory methods are invoked invoked to return a result.
        /// </remarks>
        /// <typeparam name="T">The type returned by the delegate to which the policy is applied to.</typeparam>
        /// <param name="circuitBreakerPolicy">The circuit breaker policy whose state will be checked.</param>
        /// <param name="fallbackValueFactory">A delegate to create a valid return type if the circuit's state is open or isolated.</param>
        /// <returns>The circuit breaker checker policy</returns>
        public static CircuitBreakerCheckerAsyncPolicy<T> Create<T>(
            ICircuitBreakerPolicy circuitBreakerPolicy,
            Func<CircuitBreakerState, Context, CancellationToken, Task<T>> fallbackValueFactory)
        {
            return CircuitBreakerCheckerAsyncPolicy<T>.Create(circuitBreakerPolicy, fallbackValueFactory);
        }
    }

    /// <summary>
    /// Polly policy to check if a circuit breaker is opened and avoid throwing an exception if the circuit is open/isolated.
    /// </summary>
    /// <typeparam name="T">The return type of the delegate that the policy is applied to.</typeparam>
    public class CircuitBreakerCheckerAsyncPolicy<T> : AsyncPolicy<T>
    {
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly Func<CircuitBreakerState, Context, CancellationToken, Task<T>> _fallbackValueFactory;

        internal static CircuitBreakerCheckerAsyncPolicy<T> Create(
            ICircuitBreakerPolicy circuitBreakerPolicy,
            Func<CircuitBreakerState, Context, CancellationToken, Task<T>> fallbackValueFactory)
        {
            // factory method following Polly's guidelines for custom policies: http://www.thepollyproject.org/2019/02/13/authoring-a-proactive-polly-policy-custom-policies-part-ii/
            return new CircuitBreakerCheckerAsyncPolicy<T>(circuitBreakerPolicy, fallbackValueFactory);
        }

        private CircuitBreakerCheckerAsyncPolicy(ICircuitBreakerPolicy circuitBreakerPolicy, Func<CircuitBreakerState, Context, CancellationToken, Task<T>> fallbackValueFactory)
        {
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
            _fallbackValueFactory = fallbackValueFactory ?? throw new ArgumentNullException(nameof(fallbackValueFactory));
        }

        /// <inheritdoc />
        protected override async Task<T> ImplementationAsync(
            Func<Context, CancellationToken, Task<T>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // No point in trying to make the request because the circuit breaker will throw an exception.
            // Avoid exception as indicated by https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken
            return _circuitBreakerPolicy.CircuitState switch
            {
                CircuitState.Isolated => await ExecuteFallbackValueFactoryAsync(CircuitBreakerState.Isolated),
                CircuitState.Open => await ExecuteFallbackValueFactoryAsync(CircuitBreakerState.Open),
                CircuitState.Closed or CircuitState.HalfOpen => await ExecutePolicyActionAsync(),
                _ => throw new NotImplementedException($"Unexpected circuit state: {_circuitBreakerPolicy.CircuitState}.")
            };

            async Task<T> ExecuteFallbackValueFactoryAsync(CircuitBreakerState circuitBreakerState)
            {
                var result = await _fallbackValueFactory(circuitBreakerState, context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                return result;
            }

            async Task<T> ExecutePolicyActionAsync()
            {
                var result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                return result;
            }
        }
    }
}
