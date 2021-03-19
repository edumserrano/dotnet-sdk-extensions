using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public interface ICircuitBreakerPolicyConfiguration
    {
        Task OnBreakAsync(
            CircuitBreakerOptions circuitBreakerOptions,
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak,
            Context context);

        Task OnHalfOpenAsync(CircuitBreakerOptions circuitBreakerOptions);

        Task OnResetAsync(CircuitBreakerOptions circuitBreakerOptions, Context context);
    }
}