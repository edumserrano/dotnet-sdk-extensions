using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary
{
    public class RetryPolicyExecutor
    {
        private readonly HttpClient _httpClient;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;
        private readonly List<HttpStatusCode> _transientHttpStatusCodes;

        public RetryPolicyExecutor(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _testHttpMessageHandler = testHttpMessageHandler;
            _transientHttpStatusCodes = HttpStatusCodesExtensions.GetTransientHttpStatusCodes().ToList();
        }

        public Task<HttpResponseMessage> TriggerFromExceptionAsync(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var requestPath = $"/retry/exception/{exception.GetType().Name}";
            _testHttpMessageHandler.HandleException(requestPath, exception);
            return _httpClient.GetAsync(requestPath);
        }

        public Task<HttpResponseMessage> TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode httpStatusCode)
        {
            if (!_transientHttpStatusCodes.Contains(httpStatusCode))
            {
                throw new ArgumentException($"{httpStatusCode} is not a transient HTTP status code.", nameof(httpStatusCode));
            }

            var requestPath = _testHttpMessageHandler.HandleTransientHttpStatusCode(
                requestPath: "/retry/transient-http-status-code",
                responseHttpStatusCode: httpStatusCode);
            return _httpClient.GetAsync(requestPath);
        }

        public Task<HttpResponseMessage> ExecuteCircuitBrokenHttpResponseMessageAsync()
        {
            var response = new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Open);
            var requestPath = $"/retry/circuit-broken-response/{response.GetHashCode()}";
            _testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(requestPath, StringComparison.OrdinalIgnoreCase))
                    .RespondWith(response);
            });
            return _httpClient.GetAsync(requestPath);
        }
    }
}
