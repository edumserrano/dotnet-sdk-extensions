using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.CircuitBreaker;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    public class CircuitBreakerPolicyExecutor : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly CircuitBreakerOptions _circuitBreakerOptions;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;
        private readonly string _resetRequestPath;
        private readonly List<HttpStatusCode> _transientHttpStatusCodes;

        public CircuitBreakerPolicyExecutor(
            HttpClient httpClient,
            CircuitBreakerOptions circuitBreakerOptions,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _circuitBreakerOptions = circuitBreakerOptions;
            _testHttpMessageHandler = testHttpMessageHandler;
            _resetRequestPath = HandleResetRequest();
            _transientHttpStatusCodes = HttpStatusCodesExtensions.GetTransientHttpStatusCodes().ToList();
        }

        public async Task TriggerFromExceptionAsync<TException>(Exception exception) where TException : Exception
        {
            var requestPath = $"/circuit-breaker/exception/{exception.GetType().Name}";
            _testHttpMessageHandler.HandleException(requestPath, exception);

            await TriggerCircuitBreakerFromExceptionAsync<TException>(requestPath);
        }

        public async Task TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode httpStatusCode)
        {
            var handledRequestPath = _testHttpMessageHandler.HandleTransientHttpStatusCode(
                requestPath: "/circuit-breaker/transient-http-status-code",
                responseHttpStatusCode: httpStatusCode);
            await TriggerCircuitBreakerFromTransientStatusCodeAsync(handledRequestPath, httpStatusCode);
        }

        public async Task WaitForResetAsync()
        {
            // wait for the duration of break so that the circuit goes into half open state
            await Task.Delay(TimeSpan.FromSeconds(_circuitBreakerOptions.DurationOfBreakInSecs + 0.05));
            // successful response will move the circuit breaker into closed state
            var response = await _httpClient.GetAsync(_resetRequestPath);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Unexpected status code from closed circuit. Got {response.StatusCode} but expected {HttpStatusCode.OK}.");
            }
            // make sure we transition to a new sampling window or else requests would still fall
            // in the previous sampling window where the circuit state had already been open and closed.
            await Task.Delay(TimeSpan.FromSeconds(_circuitBreakerOptions.SamplingDurationInSecs + 0.05));
        }


        /// <summary>
        /// Asserts that the circuit breaker state is open/isolated by doing an HTTP request.
        /// If the circuit breaker state is not open/isolated this will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="requestPath">The request path for the HTTP request.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the conclusion of the assertion for the circuit breaker state.
        /// </returns>
        /// <remarks>
        /// The circuit breaker policy added is a wrapped policy which joins an
        /// <see cref="AsyncCircuitBreakerPolicy{TResult}"/> and a <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/>.
        /// 
        /// The <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/> will check if the circuit is open/isolated and
        /// if so it will return <see cref="CircuitBrokenHttpResponseMessage"/> which is an http response message
        /// with 500 status code and some extra properties.
        /// </remarks>
        public async Task ShouldBeOpenAsync(string requestPath)
        {
            var response = await _httpClient.GetAsync(requestPath);
            if (response is not CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage)
            {
                throw new InvalidOperationException($"Unexpected response type from open circuit. Expected a {typeof(CircuitBrokenHttpResponseMessage)} but got a {response.GetType()} from requestPath: {requestPath}");
            }

            if (circuitBrokenHttpResponseMessage.StatusCode != HttpStatusCode.InternalServerError)
            {
                throw new InvalidOperationException($"Unexpected status code from open circuit. Got {response.StatusCode} but expected {HttpStatusCode.InternalServerError} from requestPath: {requestPath}");
            }
        }

        private string HandleResetRequest()
        {
            var handledRequestPath = "/circuit-breaker/reset";
            _testHttpMessageHandler.MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(handledRequestPath))
                    .RespondWith(new HttpResponseMessage(HttpStatusCode.OK));
            });
            return handledRequestPath;
        }

        private async Task TriggerCircuitBreakerFromTransientStatusCodeAsync(string requestPath, HttpStatusCode httpStatusCode)
        {
            if (!_transientHttpStatusCodes.Contains(httpStatusCode))
            {
                throw new ArgumentException($"{httpStatusCode} is not a transient HTTP status code.", nameof(httpStatusCode));
            }

            for (var i = 0; i < _circuitBreakerOptions.MinimumThroughput; i++)
            {
                var response = await _httpClient.GetAsync(requestPath);
                // the circuit should be closed during this loop which means it will be returning the 
                // expected status code. Once the circuit is open it starts failing fast by returning
                // a CircuitBrokenHttpResponseMessage instance whose status code is 500
                if (response.StatusCode != httpStatusCode)
                {
                    throw new InvalidOperationException($"Unexpected status code from closed circuit. Got {response.StatusCode} but expected {httpStatusCode}. Iteration {i} of minimum throughput {_circuitBreakerOptions.MinimumThroughput}");
                }
            }
        }

        private async Task TriggerCircuitBreakerFromExceptionAsync<TException>(string requestPath) where TException : Exception
        {
            for (var i = 0; i < _circuitBreakerOptions.MinimumThroughput; i++)
            {
                try
                {
                    await _httpClient.GetAsync(requestPath);
                }
                catch (TException)
                {
                    // avoids the exception being propagated in order to open the circuit once
                    // the CircuitBreakerOptions.MinimumThroughput number of requests is reached
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await WaitForResetAsync();
        }
    }
}
