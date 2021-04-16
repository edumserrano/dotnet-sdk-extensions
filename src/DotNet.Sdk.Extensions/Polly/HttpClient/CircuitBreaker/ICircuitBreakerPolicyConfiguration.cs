using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker
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