using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    internal class TimeoutPolicyAsserter
    {
        private readonly HttpClient _httpClient;
        private readonly TimeoutOptions _options;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public TimeoutPolicyAsserter(
            HttpClient httpClient,
            TimeoutOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _options = options;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public async Task HttpClientShouldContainTimeoutPolicyAsync()
        {
            await TimeoutPolicyTriggersOnTimeout();
        }

        public async Task HttpClientShouldContainTimeoutPolicyWithFallbackAsync()
        {
            await TimeoutPolicyTriggersOnTimeoutButHasFallback();
        }

        public void EventHandlerShouldReceiveExpectedEvents(
            int count,
            string httpClientName,
            TimeoutPolicyEventHandlerCalls eventHandlerCalls)
        {
            eventHandlerCalls.OnTimeoutAsyncCalls.Count.ShouldBe(count);
            foreach (var onTimeoutAsyncCall in eventHandlerCalls.OnTimeoutAsyncCalls)
            {
                onTimeoutAsyncCall.HttpClientName.ShouldBe(httpClientName);
                onTimeoutAsyncCall.TimeoutOptions.TimeoutInSecs.ShouldBe(_options.TimeoutInSecs);
            }
        }

        private async Task TimeoutPolicyTriggersOnTimeout()
        {
            await Should.ThrowAsync<TimeoutRejectedException>(() =>
            {
                return _httpClient
                    .TimeoutExecutor(_options, _testHttpMessageHandler)
                    .TriggerTimeoutPolicyAsync();
            });
        }

        private async Task TimeoutPolicyTriggersOnTimeoutButHasFallback()
        {
            var fallbackResponse = await _httpClient
                .TimeoutExecutor(_options, _testHttpMessageHandler)
                .TriggerTimeoutPolicyAsync();
            var timeoutResponse = fallbackResponse as TimeoutHttpResponseMessage;
            timeoutResponse.ShouldNotBeNull();
            timeoutResponse.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }
    }
}
