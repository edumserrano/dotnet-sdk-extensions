using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    internal class TimeoutPolicyAsserter
    {
        public async Task HttpClientShouldContainTimeoutPolicyAsync(
            HttpClient httpClient,
            TimeoutOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            await TimeoutPolicyTriggersOnTimeout(
                httpClient,
                options,
                testHttpMessageHandler);
        }

        public void EventHandlerShouldReceiveExpectedEvents(
            int count,
            string httpClientName,
            TimeoutOptions options,
            TimeoutPolicyEventHandlerCalls eventHandlerCalls)
        {
            eventHandlerCalls.OnTimeoutAsyncCalls.Count.ShouldBe(count);
            foreach (var onTimeoutAsyncCall in eventHandlerCalls.OnTimeoutAsyncCalls)
            {
                onTimeoutAsyncCall.HttpClientName.ShouldBe(httpClientName);
                onTimeoutAsyncCall.TimeoutOptions.TimeoutInSecs.ShouldBe(options.TimeoutInSecs);
            }
        }

        private Task TimeoutPolicyTriggersOnTimeout(
            HttpClient httpClient,
            TimeoutOptions timeoutOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                // this timeout is a max timeout before aborting
                // but the polly timeout policy will timeout before this happens
                builder.TimesOut(TimeSpan.FromSeconds(timeoutOptions.TimeoutInSecs + 1));
            }); 
            return Should.ThrowAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://github.com"));
        }
    }
}
