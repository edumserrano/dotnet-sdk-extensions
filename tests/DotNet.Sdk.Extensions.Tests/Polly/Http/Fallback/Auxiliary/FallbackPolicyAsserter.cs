using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    internal class FallbackPolicyAsserter
    {
        private readonly string _httpClientName;
        private readonly AsyncPolicyWrap<HttpResponseMessage>? _fallbackPolicy;
        private readonly AsyncFallbackPolicy<HttpResponseMessage>? _timeoutFallback;
        private readonly AsyncFallbackPolicy<HttpResponseMessage>? _brokenCircuitFallback;
        private readonly AsyncFallbackPolicy<HttpResponseMessage>? _abortedFallback;

        public FallbackPolicyAsserter(
            string httpClientName,
            AsyncPolicyWrap<HttpResponseMessage>? policy)
        {
            _httpClientName = httpClientName;
            _fallbackPolicy = policy;
            _timeoutFallback = _fallbackPolicy?.Outer as AsyncFallbackPolicy<HttpResponseMessage>;
            var brokenCircuitWrappedFallback = _fallbackPolicy?.Inner as AsyncPolicyWrap<HttpResponseMessage>;
            _brokenCircuitFallback = brokenCircuitWrappedFallback?.Outer as AsyncFallbackPolicy<HttpResponseMessage>;
            _abortedFallback = brokenCircuitWrappedFallback?.Inner as AsyncFallbackPolicy<HttpResponseMessage>;
        }

        public void PolicyShouldBeConfiguredAsExpected()
        {
            _fallbackPolicy.ShouldNotBeNull();
            _timeoutFallback.ShouldNotBeNull();
            _brokenCircuitFallback.ShouldNotBeNull();
            _abortedFallback.ShouldNotBeNull();

            TimeoutFallbackShouldBeConfiguredAsExpected(_timeoutFallback);
            BrokenCircuitFallbackShouldBeConfiguredAsExpected(_brokenCircuitFallback);
            AbortedFallbackShouldBeConfiguredAsExpected(_abortedFallback);
        }

        private void TimeoutFallbackShouldBeConfiguredAsExpected(AsyncFallbackPolicy<HttpResponseMessage> timeoutFallback)
        {
            var exceptionPredicates = timeoutFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(1);
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();

            var resultPredicates = timeoutFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        private void BrokenCircuitFallbackShouldBeConfiguredAsExpected(AsyncFallbackPolicy<HttpResponseMessage> brokenCircuitFallback)
        {
            var exceptionPredicates = brokenCircuitFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(2);
            exceptionPredicates.HandlesException<BrokenCircuitException>().ShouldBeTrue();
            exceptionPredicates.HandlesException(new IsolatedCircuitException("msg")).ShouldBeTrue();

            var resultPredicates = brokenCircuitFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        private void AbortedFallbackShouldBeConfiguredAsExpected(AsyncFallbackPolicy<HttpResponseMessage> abortedFallback)
        {
            var exceptionPredicates = abortedFallback.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(1);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();

            var resultPredicates = abortedFallback.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(0);
        }

        public void PolicyShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {
            _fallbackPolicy.ShouldNotBeNull();
            _timeoutFallback.ShouldNotBeNull();
            _brokenCircuitFallback.ShouldNotBeNull();
            _abortedFallback.ShouldNotBeNull();

            var timeoutOnFallbackTarget = new OnFallbackTarget(_timeoutFallback);
            ShouldTriggerPolicyEventHandler(timeoutOnFallbackTarget, _httpClientName, policyEventHandler);
            
            var brokenCircuitOnFallbackTarget = new OnFallbackTarget(_brokenCircuitFallback);
            ShouldTriggerPolicyEventHandler(brokenCircuitOnFallbackTarget, _httpClientName, policyEventHandler);
            
            var abortedOnFallbackTarget = new OnFallbackTarget(_abortedFallback);
            ShouldTriggerPolicyEventHandler(abortedOnFallbackTarget, _httpClientName, policyEventHandler);
        }

        private void ShouldTriggerPolicyEventHandler(
            OnFallbackTarget onFallbackTarget,
            string httpClientName,
            Type policyEventHandler)
        {
            onFallbackTarget.HttpClientName.ShouldBe(httpClientName);
            onFallbackTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }

        private class OnFallbackTarget
        {
            public OnFallbackTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onFallbackAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<IFallbackPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public IFallbackPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}
