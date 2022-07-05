namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

public static class HttpClientPolicyExtensions
{
    public static TimeoutPolicyExecutor TimeoutExecutor(
        this HttpClient httpClient,
        TimeoutOptions timeout,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new TimeoutPolicyExecutor(httpClient, timeout, testHttpMessageHandler);
    }

    public static RetryPolicyExecutor RetryExecutor(
        this HttpClient httpClient,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new RetryPolicyExecutor(httpClient, testHttpMessageHandler);
    }

    public static CircuitBreakerPolicyExecutor CircuitBreakerExecutor(
        this HttpClient httpClient,
        CircuitBreakerOptions options,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new CircuitBreakerPolicyExecutor(httpClient, options, testHttpMessageHandler);
    }

    public static FallbackPolicyExecutor FallbackExecutor(
        this HttpClient httpClient,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new FallbackPolicyExecutor(httpClient, testHttpMessageHandler);
    }
}
