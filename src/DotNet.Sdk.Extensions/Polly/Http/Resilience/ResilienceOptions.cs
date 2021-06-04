using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience
{
    /// <summary>
    /// Represents the options available to configure the resilience policies.
    /// </summary>
    public class ResilienceOptions
    {
        /// <summary>
        /// Creates an instance of <see cref="ResilienceOptions"/>.
        /// </summary>
        public ResilienceOptions()
        {
            CircuitBreaker = new CircuitBreakerOptions();
            Timeout = new TimeoutOptions();
            Retry = new RetryOptions();
            EnableFallbackPolicy = true;
            EnableTimeoutPolicy = true;
            EnableRetryPolicy = true;
            EnableCircuitBreakerPolicy = true;
        }

        /// <summary>
        /// Options to configure the circuit breaker policy.
        /// </summary>
        [Required]
        public CircuitBreakerOptions CircuitBreaker { get; set; }

        /// <summary>
        /// Options to configure the circuit timeout policy.
        /// </summary>
        [Required]
        public TimeoutOptions Timeout { get; set; }

        /// <summary>
        /// Options to configure the circuit retry policy.
        /// </summary>
        [Required]
        public RetryOptions Retry { get; set; }

        /// <summary>
        /// Whether or not the fallback policy added to the <see cref="HttpClient"/>.
        /// </summary>
        public bool EnableFallbackPolicy { get; set; }

        /// <summary>
        /// Whether or not the retry policy added to the <see cref="HttpClient"/>.
        /// </summary>
        public bool EnableRetryPolicy { get; set; }

        /// <summary>
        /// Whether or not the circuit breaker policy added to the <see cref="HttpClient"/>.
        /// </summary>
        public bool EnableCircuitBreakerPolicy { get; set; }

        /// <summary>
        /// Whether or not the timeout policy added to the <see cref="HttpClient"/>.
        /// </summary>
        public bool EnableTimeoutPolicy { get; set; }
    }
}
