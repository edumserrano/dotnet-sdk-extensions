using System.Threading.Tasks;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events
{
    /// <summary>
    /// Defines the events produced by the HttpClient's circuit breaker policy.
    /// </summary>
    public interface ICircuitBreakerPolicyEventHandler
    {
        /// <summary>
        /// Triggered when the circuit transitions to an <see cref="CircuitState.Open"/> state.
        /// </summary>
        /// <param name="breakEvent">Event data from the Polly's circuit breaker policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnBreakAsync(BreakEvent breakEvent);

        /// <summary>
        /// Triggered when the circuit transitions to <see cref="CircuitState.HalfOpen"/> state, ready to try action executions again.
        /// </summary>
        /// <param name="halfOpenEvent">Event data from the Polly's circuit breaker policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent);

        /// <summary>
        /// Triggered when the circuit resets to a <see cref="CircuitState.Closed"/> state.
        /// </summary>
        /// <param name="resetEvent">Event data from the Polly's circuit breaker policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnResetAsync(ResetEvent resetEvent);
    }
}
