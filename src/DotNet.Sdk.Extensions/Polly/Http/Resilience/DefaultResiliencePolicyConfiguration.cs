using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using Polly;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Polly.Http.Resilience
{
    public class DefaultResiliencePolicyConfiguration : IResiliencePolicyConfiguration
    {
        public Task OnTimeoutASync(
            TimeoutOptions timeoutOptions,
            Context context, 
            TimeSpan requestTimeout,
            Task timedOutTask,
            Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task OnRetryAsync(
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan retryDelay,
            int retryNumber,
            Context pollyContext)
        {
            return Task.CompletedTask;
        }

        public Task OnBreakAsync(
            CircuitBreakerOptions circuitBreakerOptions,
            DelegateResult<HttpResponseMessage> lastOutcome, 
            CircuitState previousState,
            TimeSpan durationOfBreak,
            Context context)
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

        public Task OnTimeoutFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }
    }
}
