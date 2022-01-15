using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.Events
{
    /// <summary>
    /// Defines the events produced by the HttpClient's fallback policy.
    /// </summary>
    public interface IFallbackPolicyEventHandler
    {
        /// <summary>
        /// Triggered when a fallback occurs due to a <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="fallbackEvent">Event data from the Polly's fallback policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnHttpRequestExceptionFallbackAsync(FallbackEvent fallbackEvent);

        /// <summary>
        /// Triggered when a fallback occurs due to a <see cref="TimeoutRejectedException"/>
        /// or a <see cref="TaskCanceledException"/> with an inner exception of <see cref="TimeoutException"/>.
        /// </summary>
        /// <param name="fallbackEvent">Event data from the Polly's fallback policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnTimeoutFallbackAsync(FallbackEvent fallbackEvent);

        /// <summary>
        /// Triggered when the fallback occurs due to the circuit breaker's state being open or isolated
        /// or due to an <see cref="IsolatedCircuitException"/> or a <see cref="BrokenCircuitException"/>.
        /// </summary>
        /// <param name="fallbackEvent">Event data from the Polly's fallback policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnBrokenCircuitFallbackAsync(FallbackEvent fallbackEvent);

        /// <summary>
        /// Triggered when a fallback occurs due to a <see cref="TaskCanceledException"/>.
        /// </summary>
        /// <param name="fallbackEvent">Event data from the Polly's fallback policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnTaskCancelledFallbackAsync(FallbackEvent fallbackEvent);
    }
}
