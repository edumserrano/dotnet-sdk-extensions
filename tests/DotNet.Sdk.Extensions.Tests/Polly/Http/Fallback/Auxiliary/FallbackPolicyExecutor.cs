using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;

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
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var requestPath = $"/fallback/exception/{exception.GetHashCode()}";
        _testHttpMessageHandler.HandleException(requestPath, exception);
        return _httpClient.GetAsync(requestPath);
    }
}
