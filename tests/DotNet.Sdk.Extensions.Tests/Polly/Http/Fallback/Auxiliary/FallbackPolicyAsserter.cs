using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Shouldly;
using System.Linq;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary
{
    internal static class FallbackPolicyAsserterExtensions
    {
        public static FallbackPolicyAsserter FallbackPolicyAsserter(
            this HttpClient httpClient,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            return new FallbackPolicyAsserter(httpClient, testHttpMessageHandler);
        }
    }

    internal class FallbackPolicyAsserter
    {
        private readonly HttpClient _httpClient;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public FallbackPolicyAsserter(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public async Task HttpClientShouldContainFallbackPolicyAsync()
        {
            await FallbackPolicyHandlesTimeoutRejectedException();
            await FallbackPolicyHandlesBrokenCircuitException();
            await FallbackPolicyHandlesIsolatedCircuitException();
            await FallbackPolicyHandlesTaskCancelledException();
            await FallbackPolicyHandlesTaskCancelledExceptionWithTimeoutException();
        }

        private async Task FallbackPolicyHandlesTaskCancelledException()
        {
            var taskCanceledException = new TaskCanceledException();
            var response = await FallbackPolicyHandlesException(taskCanceledException);
            var abortedHttpResponseMessage = response as AbortedHttpResponseMessage;
            abortedHttpResponseMessage.ShouldNotBeNull();
            abortedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            abortedHttpResponseMessage.Exception.ShouldBe(taskCanceledException);
        }

        private async Task FallbackPolicyHandlesTaskCancelledExceptionWithTimeoutException()
        {
            var timeoutException = new TimeoutException();
            var taskCanceledException = new TaskCanceledException("some msg", timeoutException);
            var response = await FallbackPolicyHandlesException(taskCanceledException);
            var timeoutHttpResponseMessage = response as TimeoutHttpResponseMessage;
            timeoutHttpResponseMessage.ShouldNotBeNull();
            timeoutHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            timeoutHttpResponseMessage.Exception.ShouldBe(taskCanceledException);
            timeoutHttpResponseMessage.Exception.InnerException.ShouldBe(timeoutException);
        }

        private async Task FallbackPolicyHandlesIsolatedCircuitException()
        {
            var isolatedCircuitException = new IsolatedCircuitException(message: string.Empty);
            var response = await FallbackPolicyHandlesException(isolatedCircuitException);
            var circuitBrokenHttpResponseMessage = response as CircuitBrokenHttpResponseMessage;
            circuitBrokenHttpResponseMessage.ShouldNotBeNull();
            circuitBrokenHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            circuitBrokenHttpResponseMessage.CircuitBreakerState.ShouldBe(CircuitBreakerState.Isolated);
            circuitBrokenHttpResponseMessage.IsolatedCircuitException.ShouldBe(isolatedCircuitException);
            circuitBrokenHttpResponseMessage.BrokenCircuitException.ShouldBeNull();
        }

        private async Task FallbackPolicyHandlesBrokenCircuitException()
        {
            var brokenCircuitException = new BrokenCircuitException();
            var response = await FallbackPolicyHandlesException(brokenCircuitException);
            var circuitBrokenHttpResponseMessage = response as CircuitBrokenHttpResponseMessage;
            circuitBrokenHttpResponseMessage.ShouldNotBeNull();
            circuitBrokenHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            circuitBrokenHttpResponseMessage.CircuitBreakerState.ShouldBe(CircuitBreakerState.Open);
            circuitBrokenHttpResponseMessage.BrokenCircuitException.ShouldBe(brokenCircuitException);
            circuitBrokenHttpResponseMessage.IsolatedCircuitException.ShouldBeNull();
        }

        private async Task FallbackPolicyHandlesTimeoutRejectedException()
        {
            var timeoutRejectedException = new TimeoutRejectedException();
            var response = await FallbackPolicyHandlesException(timeoutRejectedException);
            var timeoutHttpResponseMessage = response as TimeoutHttpResponseMessage;
            timeoutHttpResponseMessage.ShouldNotBeNull();
            timeoutHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            timeoutHttpResponseMessage.Exception.ShouldBe(timeoutRejectedException);
        }

        private Task<HttpResponseMessage> FallbackPolicyHandlesException(Exception exception)
        {
            return _httpClient
                .FallbackExecutor(_testHttpMessageHandler)
                .TriggerFromExceptionAsync(exception);
        }

        public void EventHandlerShouldReceiveExpectedEvents(
            int onTimeoutCallsCount,
            int onBrokenCircuitCallsCount,
            int onIsolatedCircuitCallsCount,
            int onTaskCancelledCallsCount,
            string httpClientName,
            FallbackPolicyEventHandlerCalls eventHandlerCalls)
        {
            eventHandlerCalls
                .OnTimeoutFallbackAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName))
                .ShouldBe(onTimeoutCallsCount);
            eventHandlerCalls
                .OnBrokenCircuitFallbackAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName) && x.Outcome.Exception is IsolatedCircuitException)
                .ShouldBe(onBrokenCircuitCallsCount);
            eventHandlerCalls
                .OnBrokenCircuitFallbackAsyncCalls // check BrokenCircuitException calls. IsolatedCircuitException are derived from BrokenCircuitException so excluding those (x.Outcome.Exception is BrokenCircuitException would also count IsolatedCircuitException)
                .Count(x => x.HttpClientName.Equals(httpClientName) && x.Outcome.Exception is not IsolatedCircuitException)
                .ShouldBe(onIsolatedCircuitCallsCount);
            eventHandlerCalls
                .OnTaskCancelledFallbackAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName))
                .ShouldBe(onTaskCancelledCallsCount);
        }
    }
}
