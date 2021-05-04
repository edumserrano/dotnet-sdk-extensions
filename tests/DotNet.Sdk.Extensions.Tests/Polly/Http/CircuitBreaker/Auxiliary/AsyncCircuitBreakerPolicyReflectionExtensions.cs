using System;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public static class AsyncCircuitBreakerPolicyReflectionExtensions
    {
        public static TimeSpan GetDurationOfBreakInSecs<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy
                .GetCircuitBreakerController()
                .GetInstanceField<TimeSpan>("_durationOfBreak");
        }

        public static double GetSamplingDurationInSecs<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy
                .GetCircuitBreakerController()
                .GetInstanceField("_metrics")
                .GetInstanceField<long>("_samplingDuration");
        }

        public static double GetFailureThreshold<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy
                .GetCircuitBreakerController()
                .GetInstanceField<double>("_failureThreshold");
        }

        public static int GetMinimumThroughput<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy
                .GetCircuitBreakerController()
                .GetInstanceField<int>("_minimumThroughput");
        }

        public static object GetCircuitBreakerController<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy.GetInstanceField("_breakerController");
        }
    }
}