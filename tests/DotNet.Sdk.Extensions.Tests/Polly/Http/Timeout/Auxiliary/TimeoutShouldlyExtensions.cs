using System;
using System.Net.Http;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public static class TimeoutShouldlyExtensions
    {
        public static void ShouldBeConfiguredAsExpected(
            this AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy,
            int timeoutInSecs)
        {
            timeoutPolicy
                .GetTimeoutStrategy()
                .ShouldBe(TimeoutStrategy.Optimistic);
            timeoutPolicy
                .GetTimeout()
                .ShouldBe(TimeSpan.FromSeconds(timeoutInSecs));
            timeoutPolicy
                .GetExceptionPredicates()
                .GetExceptionPredicatesCount()
                .ShouldBe(0);
            timeoutPolicy
                .GetResultPredicates()
                .GetResultPredicatesCount()
                .ShouldBe(0);
        }
        
        public static void ShouldTriggerPolicyEventHandler(
            this AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy,
            string httpClientName,
            int timeoutInSecs,
            Type policyConfigurationType)
        {
            var policyEventHandlerTarget = timeoutPolicy.GetOnTimeoutTarget();
            policyEventHandlerTarget.HttpClientName.ShouldBe(httpClientName);
            policyEventHandlerTarget.TimeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
            policyEventHandlerTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyConfigurationType);
        }
    }
}
