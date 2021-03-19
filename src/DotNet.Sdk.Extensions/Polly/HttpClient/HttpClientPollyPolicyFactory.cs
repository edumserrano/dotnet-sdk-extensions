using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.HttpClient.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace DotNet.Sdk.Extensions.Polly.HttpClient
{
    public delegate Task OnRetryDelegate(
        IServiceProvider serviceProvider,
        RetryOptions retryOptions,
        DelegateResult<HttpResponseMessage> outcome,
        TimeSpan retryDelay,
        int retryNumber,
        Context pollyContext);

    internal class HttpClientPollyPolicyFactory
    {
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
            string circuitName,
            double failureThreshold,
            TimeSpan samplingDuration,
            int minimumThroughput,
            TimeSpan durationOfBreak)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: failureThreshold,
                    samplingDuration: samplingDuration,
                    minimumThroughput: minimumThroughput,
                    durationOfBreak: durationOfBreak,
                    onBreak: (lastOutcome, previousState, breakDuration, context) => OnBreak(circuitName, lastOutcome, previousState, breakDuration, context),
                    onReset: context => OnReset(circuitName, context),
                    onHalfOpen: () => OnHalfOpen(circuitName));
        }

        public AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(
            IServiceProvider serviceProvider,
            RetryOptions retryOptions, 
            OnRetryDelegate onRetryDelegate)
        {
            var medianFirstRetryDelay = TimeSpan.FromSeconds(retryOptions.MedianFirstRetryDelayInSecs);
            var retryDelays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay, retryOptions.RetryCount);
            var waitAndRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(retryDelays, OnRetry(serviceProvider, retryOptions, onRetryDelegate));
            return waitAndRetryPolicy;
        }

        private Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context> OnRetry(
            IServiceProvider serviceProvider,
            RetryOptions retryOptions,
            OnRetryDelegate onRetryDelegate)
        {
            return (response, retryDelay, retry, pollyContext) =>
            {
                onRetryDelegate(serviceProvider,retryOptions, response, retryDelay, retry, pollyContext);
            };
        }

        public AsyncTimeoutPolicy<HttpResponseMessage> CreateTimeoutPolicy(TimeSpan timeout)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        }

        public AsyncPolicyWrap<HttpResponseMessage> CreateFallbackPolicy()
        {
            var timeoutFallback = Policy<HttpResponseMessage>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(new TimeoutHttpResponseMessage());
            var brokenCircuitFallback = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(new CircuitBrokenHttpResponseMessage());
            var abortedFallback = Policy<HttpResponseMessage>
                .Handle<TaskCanceledException>()
                .FallbackAsync(new AbortedHttpResponseMessage());
            return Policy.WrapAsync(timeoutFallback, brokenCircuitFallback, abortedFallback);
        }
        

        private void OnHalfOpen(string circuitName)
        {
            //_loggingService.LogCircuitBreakerOnHalfOpen<HttpClientPollyPolicyFactory>(circuitName);
        }

        private void OnReset(string circuitName, Context context)
        {
            //_loggingService.LogCircuitBreakerOnReset<HttpClientPollyPolicyFactory>(circuitName);
        }

        private void OnBreak(
            string circuitName,
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak,
            Context context)
        {
            //_loggingService.LogCircuitBreakerOnBreak<HttpClientPollyPolicyFactory>(circuitName, previousState, durationOfBreak);
        }
    }
}