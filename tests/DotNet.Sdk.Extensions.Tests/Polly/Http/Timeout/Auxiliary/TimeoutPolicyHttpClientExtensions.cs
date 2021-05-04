using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public static class TimeoutPolicyHttpClientExtensions
    {
        public static Task<HttpResponseMessage> TriggerTimeoutPolicyAsync(
            this HttpClient httpClient,
            TimeoutOptions timeoutOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            testHttpMessageHandler.MockHttpResponse(builder =>
            {
                // this timeout is a max timeout before aborting
                // but the polly timeout policy will timeout before this happens
                builder.TimesOut(TimeSpan.FromSeconds(timeoutOptions.TimeoutInSecs + 1));
            });
            return httpClient.GetAsync("https://github.com");
        }
    }
}