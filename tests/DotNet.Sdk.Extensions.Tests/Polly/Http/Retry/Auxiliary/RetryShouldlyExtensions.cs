using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public static class RetryShouldlyExtensions
    {
        public static void ShouldBeConfiguredAsExpected(
            this AsyncRetryPolicy<HttpResponseMessage> retryPolicy,
            int retryCount,
            int medianFirstRetryDelayInSecs)
        {
            AsyncRetryPolicyReflectionExtensions.GetRetryCount(retryPolicy)
                .ShouldBe(retryCount);
            AsyncRetryPolicyReflectionExtensions.GetMedianFirstRetryDelay(retryPolicy)
                .ShouldBe(TimeSpan.FromSeconds(medianFirstRetryDelayInSecs));

            var exceptionPredicates = retryPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = retryPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

        public static void ShouldTriggerPolicyEventHandler(
            this AsyncRetryPolicy<HttpResponseMessage> retryPolicy,
            string httpClientName,
            int retryCount,
            int medianFirstRetryDelayInSecs,
            Type policyEventHandler)
        {
            var policyEventHandlerTarget = AsyncRetryPolicyReflectionExtensions.GetOnRetryTarget(retryPolicy);
            policyEventHandlerTarget.HttpClientName.ShouldBe(httpClientName);
            policyEventHandlerTarget.RetryOptions.RetryCount.ShouldBe(retryCount);
            policyEventHandlerTarget.RetryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
            policyEventHandlerTarget.PolicyEventHandler
                .GetType()
                .ShouldBe(policyEventHandler);
        }
    }
}
