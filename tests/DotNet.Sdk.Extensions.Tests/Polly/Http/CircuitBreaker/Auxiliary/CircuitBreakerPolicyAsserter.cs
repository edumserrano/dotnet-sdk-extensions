using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Policies;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    internal class CircuitBreakerPolicyAsserter
    {
        private readonly string _httpClientName;
        private readonly CircuitBreakerOptions _circuitBreakerOptions;
        private readonly AsyncPolicyWrap<HttpResponseMessage>? _circuitBreakerWrappedPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage>? _circuitBreakerPolicy;
        private readonly CircuitBreakerCheckerAsyncPolicy<HttpResponseMessage>? _circuitBreakerCheckerAsyncPolicy;

        public CircuitBreakerPolicyAsserter(
            string httpClientName,
            CircuitBreakerOptions circuitBreakerOptions,
            AsyncPolicyWrap<HttpResponseMessage>? policy)
        {
            _httpClientName = httpClientName;
            _circuitBreakerOptions = circuitBreakerOptions;
            _circuitBreakerWrappedPolicy = policy;
            _circuitBreakerPolicy = _circuitBreakerWrappedPolicy?.Inner as AsyncCircuitBreakerPolicy<HttpResponseMessage>;
            _circuitBreakerCheckerAsyncPolicy = _circuitBreakerWrappedPolicy?.Outer as CircuitBreakerCheckerAsyncPolicy<HttpResponseMessage>;
        }

        public void PolicyShouldBeConfiguredAsExpected()
        {
            _circuitBreakerWrappedPolicy.ShouldNotBeNull();
            _circuitBreakerPolicy.ShouldNotBeNull();
            _circuitBreakerCheckerAsyncPolicy.ShouldNotBeNull();

            _circuitBreakerPolicy
                .GetDurationOfBreakInSecs()
                .ShouldBe(TimeSpan.FromSeconds(_circuitBreakerOptions.DurationOfBreakInSecs));
            _circuitBreakerPolicy
                .GetSamplingDurationInSecs()
                .ShouldBe(TimeSpan.FromSeconds(_circuitBreakerOptions.SamplingDurationInSecs).Ticks);
            _circuitBreakerPolicy
                .GetFailureThreshold()
                .ShouldBe(_circuitBreakerOptions.FailureThreshold);
            _circuitBreakerPolicy
                .GetMinimumThroughput()
                .ShouldBe(_circuitBreakerOptions.MinimumThroughput);

            var exceptionPredicates = _circuitBreakerPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = _circuitBreakerPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

        public void PolicyShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {
            _circuitBreakerWrappedPolicy.ShouldNotBeNull();
            _circuitBreakerPolicy.ShouldNotBeNull();
            _circuitBreakerCheckerAsyncPolicy.ShouldNotBeNull();

            var breakerController = _circuitBreakerPolicy.GetCircuitBreakerController();
            var onBreakTarget = new OnBreakTarget(breakerController);
            onBreakTarget.HttpClientName.ShouldBe(_httpClientName);
            onBreakTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(_circuitBreakerOptions.DurationOfBreakInSecs);
            onBreakTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(_circuitBreakerOptions.SamplingDurationInSecs);
            onBreakTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(_circuitBreakerOptions.FailureThreshold);
            onBreakTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(_circuitBreakerOptions.MinimumThroughput);
            onBreakTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);

            var onHalfOpenTarget = new OnHalfOpenTarget(breakerController);
            onHalfOpenTarget.HttpClientName.ShouldBe(_httpClientName);
            onHalfOpenTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(_circuitBreakerOptions.DurationOfBreakInSecs);
            onHalfOpenTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(_circuitBreakerOptions.SamplingDurationInSecs);
            onHalfOpenTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(_circuitBreakerOptions.FailureThreshold);
            onHalfOpenTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(_circuitBreakerOptions.MinimumThroughput);
            onHalfOpenTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
            
            var onResetTarget = new OnResetTarget(breakerController);
            onResetTarget.HttpClientName.ShouldBe(_httpClientName);
            onResetTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(_circuitBreakerOptions.DurationOfBreakInSecs);
            onResetTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(_circuitBreakerOptions.SamplingDurationInSecs);
            onResetTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(_circuitBreakerOptions.FailureThreshold);
            onResetTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(_circuitBreakerOptions.MinimumThroughput);
            onResetTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }

        private class OnBreakTarget
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

        private class OnResetTarget
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

        private class OnHalfOpenTarget
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
