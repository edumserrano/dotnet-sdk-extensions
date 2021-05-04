using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    internal class RetryPolicyAsserter
    {
        private readonly string _httpClientName;
        private readonly RetryOptions _retryOptions;
        private readonly AsyncRetryPolicy<HttpResponseMessage>? _retryPolicy;

        public RetryPolicyAsserter(
            string httpClientName,
            RetryOptions retryOptions,
            AsyncRetryPolicy<HttpResponseMessage>? policy)
        {
            _httpClientName = httpClientName;
            _retryOptions = retryOptions;
            _retryPolicy = policy;
        }

        public void PolicyShouldBeConfiguredAsExpected()
        {
            _retryPolicy.ShouldNotBeNull();

            _retryPolicy.GetRetryCount()
                .ShouldBe(_retryOptions.RetryCount);
            _retryPolicy.GetMedianFirstRetryDelay()
                .ShouldBe(TimeSpan.FromSeconds(_retryOptions.MedianFirstRetryDelayInSecs));

            var exceptionPredicates = _retryPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = _retryPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

        public void PolicyShouldTriggerPolicyEventHandler(Type policyEventHandler)
        {
            _retryPolicy.ShouldNotBeNull();
            var policyEventHandlerTarget = new OnRetryTarget(_retryPolicy);
            policyEventHandlerTarget.HttpClientName.ShouldBe(_httpClientName);
            policyEventHandlerTarget.RetryOptions.RetryCount.ShouldBe(_retryOptions.RetryCount);
            policyEventHandlerTarget.RetryOptions.MedianFirstRetryDelayInSecs.ShouldBe(_retryOptions.MedianFirstRetryDelayInSecs);
            policyEventHandlerTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }

        private class OnRetryTarget
        {
            public OnRetryTarget(IsPolicy policy)
            {
                var onTimeoutAsync = policy
                    .GetInstanceField("_onRetryAsync")
                    .GetInstanceProperty("Target");
                HttpClientName = onTimeoutAsync.GetInstanceField<string>("httpClientName");
                RetryOptions = onTimeoutAsync.GetInstanceField<RetryOptions>("options");
                PolicyEventHandler = onTimeoutAsync.GetInstanceField<IRetryPolicyEventHandler>("policyEventHandler");
            }

            public string HttpClientName { get; }

            public RetryOptions RetryOptions { get; }

            public IRetryPolicyEventHandler PolicyEventHandler { get; }
        }
    }
}
