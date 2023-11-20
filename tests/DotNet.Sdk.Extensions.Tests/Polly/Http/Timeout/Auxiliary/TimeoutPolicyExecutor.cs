namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

public class TimeoutPolicyExecutor(
    HttpClient httpClient,
    TimeoutOptions timeoutOptions,
    TestHttpMessageHandler testHttpMessageHandler)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TimeoutOptions _timeoutOptions = timeoutOptions;
    private readonly TestHttpMessageHandler _testHttpMessageHandler = testHttpMessageHandler;

    public Task<HttpResponseMessage> TriggerTimeoutPolicyAsync()
    {
        const string requestPath = "/timeout";
        var timeout = TimeSpan.FromSeconds(_timeoutOptions.TimeoutInSecs + 2);
        _testHttpMessageHandler.HandleTimeout(requestPath, timeout);
        return _httpClient.GetAsync(requestPath);
    }
}
