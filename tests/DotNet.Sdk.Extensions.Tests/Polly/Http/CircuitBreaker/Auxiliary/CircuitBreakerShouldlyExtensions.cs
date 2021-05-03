using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public static class CircuitBreakerShouldlyExtensions
    {
        public static void ShouldBeConfiguredAsExpected(
            this AsyncPolicyWrap<HttpResponseMessage> wrappedCircuitBreakerPolicy,
            double durationOfBreakInSecs,
            double samplingDurationInSecs,
            double failureThreshold,
            int minimumThroughput)
        {
            var asyncCircuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;
            asyncCircuitBreakerPolicy.ShouldBeConfiguredAsExpected(
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput);
        }
        
        private static void ShouldBeConfiguredAsExpected(
            this AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy,
            double durationOfBreakInSecs,
            double samplingDurationInSecs,
            double failureThreshold,
            int minimumThroughput)
        {
            circuitBreakerPolicy
                .GetDurationOfBreakInSecs()
                .ShouldBe(TimeSpan.FromSeconds(durationOfBreakInSecs));
            circuitBreakerPolicy
                .GetSamplingDurationInSecs()
                .ShouldBe(TimeSpan.FromSeconds(samplingDurationInSecs).Ticks);
            circuitBreakerPolicy
                .GetFailureThreshold()
                .ShouldBe(failureThreshold);
            circuitBreakerPolicy
                .GetMinimumThroughput()
                .ShouldBe(minimumThroughput);

            var exceptionPredicates = circuitBreakerPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = circuitBreakerPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

        public static void ShouldTriggerPolicyEventHandler(
            this AsyncPolicyWrap<HttpResponseMessage> wrappedCircuitBreakerPolicy,
            string httpClientName,
            double durationOfBreakInSecs,
            double samplingDurationInSecs,
            double failureThreshold,
            int minimumThroughput,
            Type policyEventHandler)
        {
            var asyncCircuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;
            
            var onBreakTarget = asyncCircuitBreakerPolicy.GetOnBreakTarget();
            onBreakTarget.HttpClientName.ShouldBe(httpClientName);
            onBreakTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            onBreakTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
            onBreakTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            onBreakTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            onBreakTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);  
            
            var onHalfOpenTarget = asyncCircuitBreakerPolicy.GetOnHalfOpenTarget();
            onHalfOpenTarget.HttpClientName.ShouldBe(httpClientName);
            onHalfOpenTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            onHalfOpenTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
            onHalfOpenTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            onHalfOpenTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            onHalfOpenTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);

            var onResetTarget = asyncCircuitBreakerPolicy.GetOnResetTarget();
            onResetTarget.HttpClientName.ShouldBe(httpClientName);
            onResetTarget.CircuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            onResetTarget.CircuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
            onResetTarget.CircuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            onResetTarget.CircuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            onResetTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }
    }
}
