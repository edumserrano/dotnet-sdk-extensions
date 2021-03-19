using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    internal class DefaultCircuitBreakerPolicyConfiguration : ICircuitBreakerPolicyConfiguration
    {
        public Task OnBreak(
            CircuitBreakerOptions circuitBreakerOptions, 
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnHalfOpen(CircuitBreakerOptions circuitBreakerOptions)
        {
            return Task.CompletedTask;
        }

        public Task OnReset(CircuitBreakerOptions circuitBreakerOptions, Context context)
        {
            return Task.CompletedTask;
        }
    }
}