using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.Resilience
{
    public class ResilienceOptions
    {
        public CircuitBreakerOptions CircuitBreaker { get; set; }
        
        public TimeoutOptions Timeout { get; set; }
       
        public RetryOptions Retry { get; set; }
    }
}
