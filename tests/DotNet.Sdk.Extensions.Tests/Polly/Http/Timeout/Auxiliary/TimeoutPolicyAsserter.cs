using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    internal class TimeoutPolicyAsserter
    {
        private readonly string _httpClientName;
        private readonly TimeoutOptions _timeoutOptions;
        private readonly AsyncTimeoutPolicy<HttpResponseMessage>? _timeoutPolicy;

        public TimeoutPolicyAsserter(
            string httpClientName,
            TimeoutOptions timeoutOptions,
            AsyncTimeoutPolicy<HttpResponseMessage>? policy)
        {
            _httpClientName = httpClientName;
            _timeoutOptions = timeoutOptions;
            _timeoutPolicy = policy;
        }

        public void PolicyShouldBeConfiguredAsExpected()
        {
            _timeoutPolicy.ShouldNotBeNull();
            _timeoutPolicy
                .GetTimeoutStrategy()
                .ShouldBe(TimeoutStrategy.Optimistic);
            _timeoutPolicy
                .GetTimeout()
                .ShouldBe(TimeSpan.FromSeconds(_timeoutOptions.TimeoutInSecs));
            _timeoutPolicy
                .GetExceptionPredicates()
                .GetExceptionPredicatesCount()
                .ShouldBe(0);
            _timeoutPolicy
                .GetResultPredicates()
                .GetResultPredicatesCount()
                .ShouldBe(0);
        }

        public void PolicyShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {
            _timeoutPolicy.ShouldNotBeNull();
            var policyEventHandlerTarget = new OnTimeoutTarget(_timeoutPolicy);
            policyEventHandlerTarget.HttpClientName.ShouldBe(_httpClientName);
            policyEventHandlerTarget.TimeoutOptions.TimeoutInSecs.ShouldBe(_timeoutOptions.TimeoutInSecs);
            policyEventHandlerTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }
        
        private class OnTimeoutTarget
        {
            public OnTimeoutTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onTimeoutAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                TimeoutOptions = onTimeoutAsync.GetInstanceField<TimeoutOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<ITimeoutPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public TimeoutOptions TimeoutOptions { get; }

            public ITimeoutPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}
