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
        public Task OnBreakAsync(
            CircuitBreakerOptions circuitBreakerOptions, 
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnHalfOpenAsync(CircuitBreakerOptions circuitBreakerOptions)
        {
            return Task.CompletedTask;
        }

        public Task OnResetAsync(CircuitBreakerOptions circuitBreakerOptions, Context context)
        {
            return Task.CompletedTask;
        }
    }
}