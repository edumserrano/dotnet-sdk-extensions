using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class RetryPolicyExecutor
    {
        private readonly HttpClient _httpClient;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public RetryPolicyExecutor(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public Task TriggerFromExceptionAsync(Exception exception)
        {
            var requestPath = $"/retry/exception/{exception.GetType().Name}";
            _testHttpMessageHandler.HandleException(requestPath, exception);
            return _httpClient.GetAsync(requestPath);
        }

        public async Task TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode httpStatusCode)
        {
            var requestPath = _testHttpMessageHandler.HandleTransientHttpStatusCode(
                requestPath: "/retry/transient-http-status-code",
                responseHttpStatusCode: httpStatusCode);
            await _httpClient.GetAsync(requestPath);
        }
    }
}