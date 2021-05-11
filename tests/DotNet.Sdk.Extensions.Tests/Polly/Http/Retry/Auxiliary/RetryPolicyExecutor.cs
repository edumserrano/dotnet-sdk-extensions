using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Shouldly;

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
        
        public async Task TriggerFromExceptionAsync(Exception exception)
        {
            var requestPath = $"/retry/exception/{exception.GetType().Name}";
            _testHttpMessageHandler.HandleException(requestPath, exception);
            await TriggerCircuitBreakerFromExceptionAsync(requestPath);
        }

        public async Task TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode httpStatusCode)
        {
            var handledRequestPath = _testHttpMessageHandler.HandleTransientHttpStatusCode(
                requestPath: "/retry/transient-http-status-code",
                responseHttpStatusCode: httpStatusCode);
            await TriggerRetryFromTransientStatusCodeAsync(handledRequestPath);
        }

        private async Task TriggerRetryFromTransientStatusCodeAsync(string requestPath)
        {
            await _httpClient.GetAsync(requestPath);
        }

        private async Task TriggerCircuitBreakerFromExceptionAsync(string requestPath)
        {
            await Should.ThrowAsync<Exception>(() => _httpClient.GetAsync(requestPath));
        }
    }
}