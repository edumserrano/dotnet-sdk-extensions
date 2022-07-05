namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Extensions;

/// <summary>
/// Tests for the <see cref="FallbackPolicyHttpClientBuilderExtensions"/> class.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddFallbackPolicyTests
{
    /// <summary>
    /// Tests that the <see cref="FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy(IHttpClientBuilder)"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
    /// </summary>
    [Fact]
    public async Task AddFallbackPolicy1()
    {
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddFallbackPolicy()
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        await httpClient
            .FallbackPolicyAsserter(testHttpMessageHandler)
            .HttpClientShouldContainFallbackPolicyAsync();
    }

    /// <summary>
    /// Tests that the <see cref="FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy{TPolicyEventHandler}"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IFallbackPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddFallbackPolicy2()
    {
        var fallbackPolicyEventHandlerCalls = new FallbackPolicyEventHandlerCalls();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services.AddSingleton(fallbackPolicyEventHandlerCalls);
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddFallbackPolicy<TestFallbackPolicyEventHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var fallbackPolicyAsserter = httpClient.FallbackPolicyAsserter(testHttpMessageHandler);
        await fallbackPolicyAsserter.HttpClientShouldContainFallbackPolicyAsync();
        FallbackPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            onHttpRequestExceptionCount: 1,
            onTimeoutCallsCount: 1,
            onBrokenCircuitCallsCount: 1,
            onIsolatedCircuitCallsCount: 1,
            onTaskCancelledCallsCount: 2,
            httpClientName: httpClientName,
            eventHandlerCalls: fallbackPolicyEventHandlerCalls);
    }

    /// <summary>
    /// Tests that the <see cref="FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy(IHttpClientBuilder,Func{IServiceProvider,IFallbackPolicyEventHandler})"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IFallbackPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddFallbackPolicy3()
    {
        var fallbackPolicyEventHandlerCalls = new FallbackPolicyEventHandlerCalls();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddFallbackPolicy(_ =>
            {
                return new TestFallbackPolicyEventHandler(fallbackPolicyEventHandlerCalls);
            })
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var fallbackPolicyAsserter = httpClient.FallbackPolicyAsserter(testHttpMessageHandler);
        await fallbackPolicyAsserter.HttpClientShouldContainFallbackPolicyAsync();
        FallbackPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            onHttpRequestExceptionCount: 1,
            onTimeoutCallsCount: 1,
            onBrokenCircuitCallsCount: 1,
            onIsolatedCircuitCallsCount: 1,
            onTaskCancelledCallsCount: 2,
            httpClientName: httpClientName,
            eventHandlerCalls: fallbackPolicyEventHandlerCalls);
    }

    /// <summary>
    /// This tests that the policies added to the <see cref="HttpClient"/> by the
    /// FallbackPolicyHttpClientBuilderExtensions.AddFallbackPolicy methods are unique.
    ///
    /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
    /// the policy it would trigger for all.
    /// </summary>
    [Fact]
    public void AddFallbackPolicyUniquePolicyPerHttpClient()
    {
        AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy1 = null;
        AsyncPolicyWrap<HttpResponseMessage>? fallbackPolicy2 = null;
        var services = new ServiceCollection();
        services
            .AddHttpClient("GitHub")
            .AddFallbackPolicy()
            .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
            {
                fallbackPolicy1 = httpMessageHandlerBuilder
                    .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                    .FirstOrDefault();
            });
        services
            .AddHttpClient("Microsoft")
            .AddFallbackPolicy()
            .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
            {
                fallbackPolicy2 = httpMessageHandlerBuilder
                    .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                    .FirstOrDefault();
            });

        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.InstantiateNamedHttpClient("GitHub");
        serviceProvider.InstantiateNamedHttpClient("Microsoft");

        fallbackPolicy1.ShouldNotBeNull();
        fallbackPolicy2.ShouldNotBeNull();
        ReferenceEquals(fallbackPolicy1, fallbackPolicy2).ShouldBeFalse();
        fallbackPolicy1.PolicyKey.ShouldNotBe(fallbackPolicy2.PolicyKey);
    }
}
