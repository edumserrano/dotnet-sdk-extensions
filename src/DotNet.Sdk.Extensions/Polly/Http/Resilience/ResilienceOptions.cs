using System.ComponentModel.DataAnnotations;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience
{
    public class ResilienceOptions
    {
        public ResilienceOptions()
        {
            CircuitBreaker = new CircuitBreakerOptions();
            Timeout = new TimeoutOptions();
            Retry = new RetryOptions();
            EnableFallbackPolicy = true;
        }

        [Required]
        public CircuitBreakerOptions CircuitBreaker { get; set; }

        [Required]
        public TimeoutOptions Timeout { get; set; }

        [Required]
        public RetryOptions Retry { get; set; }

        public bool EnableFallbackPolicy { get; set; }
    }
}
