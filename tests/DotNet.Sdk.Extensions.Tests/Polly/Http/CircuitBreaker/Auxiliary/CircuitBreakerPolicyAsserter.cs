using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Polly.Timeout;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary
{
    internal static class CircuitBreakerPolicyAsserterExtensions
    {
        public static CircuitBreakerPolicyAsserter CircuitBreakerPolicyAsserter(
            this HttpClient httpClient,
            CircuitBreakerOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            return new CircuitBreakerPolicyAsserter(httpClient, options, testHttpMessageHandler);
        }
    }

    internal class CircuitBreakerPolicyAsserter
    {
        private readonly HttpClient _httpClient;
        private readonly CircuitBreakerOptions _options;
        private readonly TestHttpMessageHandler _testHttpMessageHandler;

        public CircuitBreakerPolicyAsserter(
            HttpClient httpClient,
            CircuitBreakerOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            _httpClient = httpClient;
            _options = options;
            _testHttpMessageHandler = testHttpMessageHandler;
        }

        public async Task HttpClientShouldContainCircuitBreakerPolicyAsync()
        {
            await CircuitBreakerPolicyHandlesTransientStatusCodes();
            await CircuitBreakerPolicyHandlesException<HttpRequestException>();
            await CircuitBreakerPolicyHandlesException<TimeoutRejectedException>();
            await CircuitBreakerPolicyHandlesException<TaskCanceledException>();
        }

        public void EventHandlerShouldReceiveExpectedEvents(
            int count,
            string httpClientName,
            CircuitBreakerPolicyEventHandlerCalls eventHandlerCalls)
        {
            eventHandlerCalls
                .OnBreakAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
                .ShouldBe(count);
            eventHandlerCalls
                .OnResetAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
                .ShouldBe(count);
            eventHandlerCalls
                .OnHalfOpenAsyncCalls
                .Count(x => x.HttpClientName.Equals(httpClientName, StringComparison.Ordinal)
                            && x.CircuitBreakerOptions.DurationOfBreakInSecs.Equals(_options.DurationOfBreakInSecs)
                            && x.CircuitBreakerOptions.FailureThreshold.Equals(_options.FailureThreshold)
                            && x.CircuitBreakerOptions.MinimumThroughput.Equals(_options.MinimumThroughput)
                            && x.CircuitBreakerOptions.SamplingDurationInSecs.Equals(_options.SamplingDurationInSecs))
                .ShouldBe(count);
        }

        private async Task CircuitBreakerPolicyHandlesTransientStatusCodes()
        {
            foreach (var transientHttpStatusCode in HttpStatusCodesExtensions.GetTransientHttpStatusCodes())
            {
                await using var circuitBreaker = _httpClient.CircuitBreakerExecutor(_options, _testHttpMessageHandler);
                await circuitBreaker.TriggerFromTransientHttpStatusCodeAsync(transientHttpStatusCode);
                await circuitBreaker.ShouldBeOpenAsync($"/circuit-breaker/transient-http-status-code/{transientHttpStatusCode}");
            }
        }

        private Task CircuitBreakerPolicyHandlesException<TException>() where TException : Exception
        {
            var exception = Activator.CreateInstance<TException>();
            return CircuitBreakerPolicyHandlesException(exception);
        }

        private async Task CircuitBreakerPolicyHandlesException<TException>(TException exception) where TException : Exception
        {
            await using var circuitBreaker = _httpClient.CircuitBreakerExecutor(_options, _testHttpMessageHandler);
            await circuitBreaker.TriggerFromExceptionAsync<TException>(exception);
            await circuitBreaker.ShouldBeOpenAsync($"/circuit-breaker/exception/{exception.GetType().Name}");
        }
    }
}
