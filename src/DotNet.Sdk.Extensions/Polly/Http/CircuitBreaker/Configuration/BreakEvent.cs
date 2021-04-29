using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Configuration
{
    public class BreakEvent
    {
        internal BreakEvent(
            string httpClientName, 
            CircuitBreakerOptions circuitBreakerOptions,
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState, 
            TimeSpan durationOfBreak,
            Context context)
        {
            HttpClientName = httpClientName;
            CircuitBreakerOptions = circuitBreakerOptions;
            LastOutcome = lastOutcome;
            PreviousState = previousState;
            DurationOfBreak = durationOfBreak;
            Context = context;
        }

        public string HttpClientName { get; }

        public CircuitBreakerOptions CircuitBreakerOptions { get; }
        
        public DelegateResult<HttpResponseMessage> LastOutcome { get; }
        
        public CircuitState PreviousState { get; }

        public TimeSpan DurationOfBreak { get; }
        
        public Context Context { get; }
    }
}