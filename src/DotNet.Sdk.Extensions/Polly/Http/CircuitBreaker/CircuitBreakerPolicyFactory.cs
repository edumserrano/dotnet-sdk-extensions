using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly.Wrap;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker
{
    internal static class CircuitBreakerPolicyFactory
    {
        public static AsyncPolicyWrap<HttpResponseMessage> CreateCircuitBreakerPolicy(
            string httpClientName,
            CircuitBreakerOptions options,
            ICircuitBreakerPolicyEventHandler policyEventHandler)
        {
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .Or<TaskCanceledException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: options.FailureThreshold,
                    samplingDuration: TimeSpan.FromSeconds(options.SamplingDurationInSecs),
                    minimumThroughput: options.MinimumThroughput,
                    durationOfBreak: TimeSpan.FromSeconds(options.DurationOfBreakInSecs),
                    onBreak: async (lastOutcome, previousState, breakDuration, context) =>
                    {
                        var breakEvent = new BreakEvent(
                            httpClientName,
                            options,
                            lastOutcome,
                            previousState,
                            breakDuration,
                            context);
                        await policyEventHandler.OnBreakAsync(breakEvent);
                    },
                    onReset: async context =>
                    {
                        var resetEvent = new ResetEvent(httpClientName, options, context);
                        await policyEventHandler.OnResetAsync(resetEvent);
                    },
                    onHalfOpen: async () =>
                    {
                        var halfOpenEvent = new HalfOpenEvent(httpClientName, options);
                        await policyEventHandler.OnHalfOpenAsync(halfOpenEvent);
                    });
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy<HttpResponseMessage>.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (context, cancellationToken) => Task.FromResult<HttpResponseMessage>(new CircuitBrokenHttpResponseMessage()));
            return Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
        }
    }
}
