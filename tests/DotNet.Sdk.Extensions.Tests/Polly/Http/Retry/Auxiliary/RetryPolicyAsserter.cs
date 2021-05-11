﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    internal class RetryPolicyAsserter
    {
        public async Task HttpClientShouldContainRetryPolicyAsync(
            HttpClient httpClient,
            RetryOptions options,
            NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await RetryPolicyHandlesTransientStatusCodes(
                httpClient,
                options,
                numberOfCallsDelegatingHandler,
                testHttpMessageHandler);
            await RetryPolicyHandlesHttpRequestException(
                httpClient,
                options,
                numberOfCallsDelegatingHandler,
                testHttpMessageHandler);
            await RetryPolicyHandlesTimeoutRejectedException(
                httpClient,
                options,
                numberOfCallsDelegatingHandler,
                testHttpMessageHandler);
            await RetryPolicyHandlesTaskCancelledException(
                httpClient,
                options,
                numberOfCallsDelegatingHandler,
                testHttpMessageHandler);
        }

        public void EventHandlerShouldReceiveExpectedEvents(
            int count,
            string httpClientName,
            RetryOptions options,
            RetryPolicyEventHandlerCalls eventHandlerCalls)
        {
            eventHandlerCalls.OnRetryAsyncCalls.Count.ShouldBe(count);
            foreach (var onRetryAsyncCall in eventHandlerCalls.OnRetryAsyncCalls)
            {
                onRetryAsyncCall.HttpClientName.ShouldBe(httpClientName);
                onRetryAsyncCall.RetryOptions.RetryCount.ShouldBe(options.RetryCount);
                onRetryAsyncCall.RetryOptions.MedianFirstRetryDelayInSecs.ShouldBe(options.MedianFirstRetryDelayInSecs);
            }
        }

        private async Task RetryPolicyHandlesTransientStatusCodes(
            HttpClient httpClient,
            RetryOptions retryOptions,
            NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await TriggerRetryPolicyFromTransientHttpStatusCodeAsync(httpClient, testHttpMessageHandler);
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(retryOptions.RetryCount + 1);
            numberOfCallsDelegatingHandler.Reset();
        }

        private async Task RetryPolicyHandlesHttpRequestException(
            HttpClient httpClient,
            RetryOptions retryOptions,
            NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await Should.ThrowAsync<HttpRequestException>(() => TriggerRetryPolicyFromHttpRequestExceptionAsync(httpClient, testHttpMessageHandler));
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(retryOptions.RetryCount + 1);
            numberOfCallsDelegatingHandler.Reset();
        }
        
        private async Task RetryPolicyHandlesTimeoutRejectedException(
            HttpClient httpClient,
            RetryOptions retryOptions,
            NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await Should.ThrowAsync<TimeoutRejectedException>(() => TriggerRetryPolicyFromTimeoutRejectedExceptionAsync(httpClient, testHttpMessageHandler));
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(retryOptions.RetryCount + 1);
            numberOfCallsDelegatingHandler.Reset();
        }

        private async Task RetryPolicyHandlesTaskCancelledException(
            HttpClient httpClient,
            RetryOptions retryOptions,
            NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await Should.ThrowAsync<TaskCanceledException>(() => TriggerRetryPolicyFromTaskCancelledExceptionAsync(httpClient, testHttpMessageHandler));
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(retryOptions.RetryCount + 1);
            numberOfCallsDelegatingHandler.Reset();
        }

        private Task<HttpResponseMessage> TriggerRetryPolicyFromTransientHttpStatusCodeAsync(
            HttpClient httpClient,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains("/transient-http-status-code"))
                    .RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });
            return httpClient.GetAsync("https://github.com/transient-http-status-code");
        }

        private Task<HttpResponseMessage> TriggerRetryPolicyFromHttpRequestExceptionAsync(
            HttpClient httpClient,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains("/http-request-exception"))
                    .RespondWith(_ => throw new HttpRequestException());
            });
            return httpClient.GetAsync("https://github.com/http-request-exception");
        }

        private Task<HttpResponseMessage> TriggerRetryPolicyFromTaskCancelledExceptionAsync(
            HttpClient httpClient,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains("/task-cancelled-exception"))
                    .RespondWith(_ => throw new TaskCanceledException());
            });
            return httpClient.GetAsync("https://github.com/task-cancelled-exception");
        }
        
        private Task<HttpResponseMessage> TriggerRetryPolicyFromTimeoutRejectedExceptionAsync(
            HttpClient httpClient,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains("/timeout-rejected-exception"))
                    .RespondWith(_ => throw new TimeoutRejectedException());
            });
            return httpClient.GetAsync("https://github.com/timeout-rejected-exception");
        }
    }
}
