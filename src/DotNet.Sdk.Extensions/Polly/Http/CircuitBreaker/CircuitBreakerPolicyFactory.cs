using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker
{
    internal static class CircuitBreakerPolicyFactory
    {
        public static IsPolicy CreateCircuitBreakerPolicy(
            CircuitBreakerOptions options,
            ICircuitBreakerPolicyConfiguration policyConfiguration)
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
                        await policyConfiguration.OnBreakAsync(options, lastOutcome, previousState, breakDuration, context);
                    },
                    onReset: async context =>
                    {
                        await policyConfiguration.OnResetAsync(options, context);
                    },
                    onHalfOpen: async () =>
                    {
                        await policyConfiguration.OnHalfOpenAsync(options);
                    });
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy<HttpResponseMessage>.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (context, cancellationToken) => Task.FromResult<HttpResponseMessage>(new CircuitBrokenHttpResponseMessage()));
            var finalPolicy = Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
            return finalPolicy;
        }
    }
}
