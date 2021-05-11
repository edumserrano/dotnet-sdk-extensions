using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class HttpClientPolicyExtensions
    {
        public static CircuitBreakerPolicyExecutor CircuitBreaker(
            this HttpClient httpClient,
            CircuitBreakerOptions options,
            TestHttpMessageHandler testHttpMessageHandler)
        {
            return new CircuitBreakerPolicyExecutor(httpClient, options, testHttpMessageHandler);
        }
    }
}
