﻿using System;
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
        private readonly HttpClient _httpClient;
        private readonly RetryOptions _options;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public RetryPolicyAsserter(
            HttpClient httpClient,
            RetryOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _options = options;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public async Task HttpClientShouldContainRetryPolicyAsync(NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler)
        {
            await RetryPolicyHandlesTransientStatusCodes(numberOfCallsDelegatingHandler);
            await RetryPolicyHandlesException<HttpRequestException>(numberOfCallsDelegatingHandler);
            await RetryPolicyHandlesException<TimeoutRejectedException>(numberOfCallsDelegatingHandler);
            await RetryPolicyHandlesException<TaskCanceledException>(numberOfCallsDelegatingHandler);
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

        private async Task RetryPolicyHandlesTransientStatusCodes(NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler)
        {
            var retryExecutor = _httpClient.RetryExecutor(_testHttpMessageHandler);
            foreach (var transientHttpStatusCode in HttpStatusCodesExtensions.GetTransientHttpStatusCodes())
            {
                await retryExecutor.TriggerFromTransientHttpStatusCodeAsync(transientHttpStatusCode);
                numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(_options.RetryCount + 1);
                numberOfCallsDelegatingHandler.Reset();
            }
        }

        private Task RetryPolicyHandlesException<TException>(NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler)
            where TException : Exception
        {
            var exception = Activator.CreateInstance<TException>();
            return RetryPolicyHandlesException(exception, numberOfCallsDelegatingHandler);
        }

        private async Task RetryPolicyHandlesException(Exception exception, NumberOfCallsDelegatingHandler numberOfCallsDelegatingHandler)
        {
            await _httpClient
                .RetryExecutor(_testHttpMessageHandler)
                .TriggerFromExceptionAsync(exception);
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(_options.RetryCount + 1);
            numberOfCallsDelegatingHandler.Reset();
        }
    }
}
