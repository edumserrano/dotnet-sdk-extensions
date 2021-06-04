using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary
{
    public class TimeoutPolicyExecutor
    {
        private readonly HttpClient _httpClient;
        private readonly TimeoutOptions _timeoutOptions;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public TimeoutPolicyExecutor(
            HttpClient httpClient,
            TimeoutOptions timeoutOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _timeoutOptions = timeoutOptions;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public Task<HttpResponseMessage> TriggerTimeoutPolicyAsync()
        {
            var requestPath = "/timeout";
            var timeout = TimeSpan.FromSeconds(_timeoutOptions.TimeoutInSecs + 1);
            _testHttpMessageHandler.HandleTimeout(requestPath, timeout);
            return _httpClient.GetAsync(requestPath);
        }
    }
}