namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;

public class RetryPolicyExecutor
{
    private readonly HttpClient _httpClient;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;
    private readonly List<HttpStatusCode> _transientHttpStatusCodes;

    public RetryPolicyExecutor(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
    {
        _httpClient = httpClient;
        _testHttpMessageHandler = testHttpMessageHandler;
        _transientHttpStatusCodes = [.. HttpStatusCodesExtensions.GetTransientHttpStatusCodes()];
    }

    public Task<HttpResponseMessage> TriggerFromExceptionAsync(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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

    public async Task<HttpResponseMessage> ExecuteCircuitBrokenHttpResponseMessageAsync()
    {
        var requestPath = $"/retry/circuit-broken-response/{Guid.NewGuid()}";
        _testHttpMessageHandler.MockHttpResponse(builder =>
        {
            builder
                .Where(httpRequestMessage => httpRequestMessage.RequestUri!.ToString().Contains(requestPath, StringComparison.OrdinalIgnoreCase))
                .RespondWith(() => new CircuitBrokenHttpResponseMessage(CircuitBreakerState.Open));
        });
        return await _httpClient.GetAsync(requestPath);
    }
}
