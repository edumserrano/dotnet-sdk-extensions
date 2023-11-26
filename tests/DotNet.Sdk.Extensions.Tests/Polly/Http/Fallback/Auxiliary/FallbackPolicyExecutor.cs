namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;

public class FallbackPolicyExecutor
{
    private readonly HttpClient _httpClient;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;

    public FallbackPolicyExecutor(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
    {
        _httpClient = httpClient;
        _testHttpMessageHandler = testHttpMessageHandler;
    }

    public Task<HttpResponseMessage> TriggerFromExceptionAsync(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var requestPath = $"/fallback/exception/{exception.GetHashCode()}";
        _testHttpMessageHandler.HandleException(requestPath, exception);
        return _httpClient.GetAsync(requestPath);
    }
}
