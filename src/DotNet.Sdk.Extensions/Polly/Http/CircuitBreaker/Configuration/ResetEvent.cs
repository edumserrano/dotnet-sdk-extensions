using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration
{
    public class ResetEvent
    {
        internal ResetEvent(
            string httpClientName, 
            CircuitBreakerOptions circuitBreakerOptions,
            Context context)
        {
            HttpClientName = httpClientName;
            CircuitBreakerOptions = circuitBreakerOptions;
            Context = context;
        }

        public string HttpClientName { get; }

        public CircuitBreakerOptions CircuitBreakerOptions { get; }
        
        public Context Context { get; }
    }
}