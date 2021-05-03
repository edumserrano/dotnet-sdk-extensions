using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    public static class FallbackShouldlyExtensions
    {
        public static void ShouldBeConfiguredAsExpected(this AsyncPolicyWrap<HttpResponseMessage> wrappedFallbackPolicy)
        {
            var fallbackPolicy = new FallbackPolicyUnWrapper(wrappedFallbackPolicy);
            fallbackPolicy.TimeoutFallback.TimeoutFallbackShouldBeConfiguredAsExpected();
            fallbackPolicy.BrokenCircuitFallback.BrokenCircuitFallbackShouldBeConfiguredAsExpected();
            fallbackPolicy.AbortedFallback.AbortedFallbackShouldBeConfiguredAsExpected();
        }

        private static void TimeoutFallbackShouldBeConfiguredAsExpected(this AsyncFallbackPolicy<HttpResponseMessage> timeoutFallback)
        {
            var exceptionPredicates = timeoutFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(1);
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();

            var resultPredicates = timeoutFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        private static void BrokenCircuitFallbackShouldBeConfiguredAsExpected(this AsyncFallbackPolicy<HttpResponseMessage> brokenCircuitFallback)
        {
            var exceptionPredicates = brokenCircuitFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(2);
            exceptionPredicates.HandlesException<BrokenCircuitException>().ShouldBeTrue();
            exceptionPredicates.HandlesException(new IsolatedCircuitException("msg")).ShouldBeTrue();

            var resultPredicates = brokenCircuitFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        private static void AbortedFallbackShouldBeConfiguredAsExpected(this AsyncFallbackPolicy<HttpResponseMessage> abortedFallback)
        {
            var exceptionPredicates = abortedFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(1);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();

            var resultPredicates = abortedFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        public static void ShouldTriggerPolicyEventHandler(
            this AsyncPolicyWrap<HttpResponseMessage> wrappedFallbackPolicy,
            string httpClientName,
            Type policyEventHandler)
        {
            var fallbackPolicy = new FallbackPolicyUnWrapper(wrappedFallbackPolicy);
            fallbackPolicy.TimeoutFallback.ShouldTriggerPolicyEventHandler(httpClientName, policyEventHandler);
            fallbackPolicy.BrokenCircuitFallback.ShouldTriggerPolicyEventHandler(httpClientName, policyEventHandler);
            fallbackPolicy.AbortedFallback.ShouldTriggerPolicyEventHandler(httpClientName, policyEventHandler);
        }

        private static void ShouldTriggerPolicyEventHandler(
            this AsyncFallbackPolicy<HttpResponseMessage> fallbackPolicy,
            string httpClientName,
            Type policyEventHandler)
        {
            var eventHandlerTarget = fallbackPolicy.GetOnFallbackTarget();
            eventHandlerTarget.HttpClientName.ShouldBe(httpClientName);
            eventHandlerTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }

        private class FallbackPolicyUnWrapper
        {
            public FallbackPolicyUnWrapper(AsyncPolicyWrap<HttpResponseMessage> wrappedFallbackPolicy)
            {
                TimeoutFallback = (AsyncFallbackPolicy<HttpResponseMessage>)wrappedFallbackPolicy.Outer;
                var wrappedBrokenCircuitFallback = (AsyncPolicyWrap<HttpResponseMessage>)wrappedFallbackPolicy.Inner;
                BrokenCircuitFallback = (AsyncFallbackPolicy<HttpResponseMessage>)wrappedBrokenCircuitFallback.Outer;
                AbortedFallback = (AsyncFallbackPolicy<HttpResponseMessage>)wrappedBrokenCircuitFallback.Inner;
            }

            public AsyncFallbackPolicy<HttpResponseMessage> AbortedFallback { get; set; }

            public AsyncFallbackPolicy<HttpResponseMessage> BrokenCircuitFallback { get; set; }

            public AsyncFallbackPolicy<HttpResponseMessage> TimeoutFallback { get; set; }
        }
    }
}
