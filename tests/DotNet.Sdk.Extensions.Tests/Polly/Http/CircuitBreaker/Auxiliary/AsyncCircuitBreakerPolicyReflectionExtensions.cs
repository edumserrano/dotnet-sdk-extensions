using System;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
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

        private static object GetCircuitBreakerController<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            return policy.GetInstanceField("_breakerController");
        }
        
        public static OnBreakTarget GetOnBreakTarget<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            var breakerController = policy.GetCircuitBreakerController();
            return new OnBreakTarget(breakerController);
        }

        public static OnResetTarget GetOnResetTarget<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            var breakerController = policy.GetCircuitBreakerController();
            return new OnResetTarget(breakerController);
        }
        
        public static OnHalfOpenTarget GetOnHalfOpenTarget<T>(this AsyncCircuitBreakerPolicy<T> policy)
        {
            var breakerController = policy.GetCircuitBreakerController();
            return new OnHalfOpenTarget(breakerController);
        }
        
        public class OnBreakTarget
        {
            public OnBreakTarget(object breakerController)
            {
                var onTimeoutAsync = breakerController
                    .GetInstanceField("_onBreak")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                CircuitBreakerOptions = onTimeoutAsync.GetInstanceField<CircuitBreakerOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<ICircuitBreakerPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public CircuitBreakerOptions CircuitBreakerOptions { get; }

            public ICircuitBreakerPolicyEventHandler PolicyEventHandler { get; }
        }

        public class OnResetTarget
        {
            public OnResetTarget(object breakerController)
            {
                var onTimeoutAsync = breakerController
                    .GetInstanceField("_onReset")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                CircuitBreakerOptions = onTimeoutAsync.GetInstanceField<CircuitBreakerOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<ICircuitBreakerPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public CircuitBreakerOptions CircuitBreakerOptions { get; }

            public ICircuitBreakerPolicyEventHandler PolicyEventHandler { get; }
        }

        public class OnHalfOpenTarget
        {
            public OnHalfOpenTarget(object breakerController)
            {
                var onTimeoutAsync = breakerController
                    .GetInstanceField("_onHalfOpen")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                CircuitBreakerOptions = onTimeoutAsync.GetInstanceField<CircuitBreakerOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<ICircuitBreakerPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public CircuitBreakerOptions CircuitBreakerOptions { get; }

            public ICircuitBreakerPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}