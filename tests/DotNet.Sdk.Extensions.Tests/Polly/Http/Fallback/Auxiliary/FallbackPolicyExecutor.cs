namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;

public class FallbackPolicyExecutor(HttpClient httpClient, TestHttpMessageHandler testHttpMessageHandler)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TestHttpMessageHandler _testHttpMessageHandler = testHttpMessageHandler;

    public Task<HttpResponseMessage> TriggerFromExceptionAsync(Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var requestPath = $"/fallback/exception/{exception.GetHashCode()}";
        _testHttpMessageHandler.HandleException(requestPath, exception);
        return _httpClient.GetAsync(requestPath);
    }
}
