namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;

internal static class TimeoutPolicyAsserterExtensions
{
    public static TimeoutPolicyAsserter TimeoutPolicyAsserter(
        this HttpClient httpClient,
        TimeoutOptions options,
        TestHttpMessageHandler testHttpMessageHandler)
    {
        return new TimeoutPolicyAsserter(httpClient, options, testHttpMessageHandler);
    }
}

internal sealed class TimeoutPolicyAsserter(
    HttpClient httpClient,
    TimeoutOptions options,
    TestHttpMessageHandler testHttpMessageHandler)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TimeoutOptions _options = options;
    private readonly TestHttpMessageHandler _testHttpMessageHandler = testHttpMessageHandler;

    public Task HttpClientShouldContainTimeoutPolicyAsync()
    {
        return TimeoutPolicyTriggersOnTimeout();
    }

    public void EventHandlerShouldReceiveExpectedEvents(
        int count,
        string httpClientName,
        TimeoutPolicyEventHandlerCalls eventHandlerCalls)
    {
        eventHandlerCalls.OnTimeoutAsyncCalls.Count.ShouldBe(count);
        foreach (var onTimeoutAsyncCall in eventHandlerCalls.OnTimeoutAsyncCalls)
        {
            onTimeoutAsyncCall.HttpClientName.ShouldBe(httpClientName);
            onTimeoutAsyncCall.TimeoutOptions.TimeoutInSecs.ShouldBe(_options.TimeoutInSecs);
        }
    }

    private Task TimeoutPolicyTriggersOnTimeout()
    {
        return Should.ThrowAsync<TimeoutRejectedException>(() =>
         {
             return _httpClient
                 .TimeoutExecutor(_options, _testHttpMessageHandler)
                 .TriggerTimeoutPolicyAsync();
         });
    }
}
