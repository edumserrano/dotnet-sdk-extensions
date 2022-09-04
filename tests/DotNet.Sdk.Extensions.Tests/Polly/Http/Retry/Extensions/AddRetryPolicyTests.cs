namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions;

/// <summary>
/// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
/// </summary>
[Trait("Category", XUnitCategories.Polly)]
public class AddRetryPolicyTests
{
    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,Action{RetryOptions})"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy1()
    {
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy(options =>
            {
                options.RetryCount = retryOptions.RetryCount;
                options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
            })
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        await httpClient
            .RetryPolicyAsserter(retryOptions, testHttpMessageHandler)
            .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
    }

    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,string)"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy2()
    {
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientRetryOptions(optionsName)
            .Configure(options =>
            {
                options.RetryCount = retryOptions.RetryCount;
                options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
            });
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy(optionsName)
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        await httpClient
            .RetryPolicyAsserter(retryOptions, testHttpMessageHandler)
            .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
    }

    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{RetryOptions})"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IRetryPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy3()
    {
        var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        var services = new ServiceCollection();
        services.AddSingleton(retryPolicyEventHandlerCalls);
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy<TestRetryPolicyEventHandler>(options =>
            {
                options.RetryCount = retryOptions.RetryCount;
                options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
            })
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var retryPolicyAsserter = httpClient.RetryPolicyAsserter(retryOptions, testHttpMessageHandler);
        await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
            httpClientName: httpClientName,
            eventHandlerCalls: retryPolicyEventHandlerCalls);
    }

    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IRetryPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy4()
    {
        var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services.AddSingleton(retryPolicyEventHandlerCalls);
        services
            .AddHttpClientRetryOptions(optionsName)
            .Configure(options =>
            {
                options.RetryCount = retryOptions.RetryCount;
                options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
            });
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy<TestRetryPolicyEventHandler>(optionsName)
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var retryPolicyAsserter = httpClient.RetryPolicyAsserter(retryOptions, testHttpMessageHandler);
        await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
            httpClientName: httpClientName,
            eventHandlerCalls: retryPolicyEventHandlerCalls);
    }

    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,string,Func{IServiceProvider,IRetryPolicyEventHandler})"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IRetryPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy5()
    {
        var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        const string optionsName = "GitHubOptions";
        var services = new ServiceCollection();
        services
            .AddHttpClientRetryOptions(optionsName)
            .Configure(options =>
            {
                options.RetryCount = retryOptions.RetryCount;
                options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
            });
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy(optionsName, _ =>
            {
                return new TestRetryPolicyEventHandler(retryPolicyEventHandlerCalls);
            })
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var retryPolicyAsserter = httpClient.RetryPolicyAsserter(retryOptions, testHttpMessageHandler);
        await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
            httpClientName: httpClientName,
            eventHandlerCalls: retryPolicyEventHandlerCalls);
    }

    /// <summary>
    /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,Action{RetryOptions},Func{IServiceProvider,IRetryPolicyEventHandler})"/>
    /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
    ///
    /// This also tests that the <see cref="IRetryPolicyEventHandler"/> events are triggered with the correct values.
    /// </summary>
    [Fact]
    public async Task AddRetryPolicy6()
    {
        var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
        using var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
        var testHttpMessageHandler = new TestHttpMessageHandler();
        const string httpClientName = "GitHub";
        var retryOptions = new RetryOptions
        {
            RetryCount = 2,
            MedianFirstRetryDelayInSecs = 0.01,
        };
        var services = new ServiceCollection();
        services
            .AddHttpClient(httpClientName)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
            .AddRetryPolicy(
                configureOptions: options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                },
                eventHandlerFactory: _ =>
                {
                    return new TestRetryPolicyEventHandler(retryPolicyEventHandlerCalls);
                })
            .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
            .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
        var retryPolicyAsserter = httpClient.RetryPolicyAsserter(retryOptions, testHttpMessageHandler);
        await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
            count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
            httpClientName: httpClientName,
            eventHandlerCalls: retryPolicyEventHandlerCalls);
    }

    /// <summary>
    /// This tests that the policies added to the <see cref="HttpClient"/> by the
    /// RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods are unique.
    ///
    /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
    /// the policy it would trigger for all.
    /// </summary>
    [Fact]
    public void AddRetryPolicyUniquePolicyPerHttpClient()
    {
        AsyncRetryPolicy<HttpResponseMessage>? retryPolicy1 = null;
        AsyncRetryPolicy<HttpResponseMessage>? retryPolicy2 = null;
        var services = new ServiceCollection();
        services
            .AddHttpClient("GitHub")
            .AddRetryPolicy(options =>
            {
                options.RetryCount = 2;
                options.MedianFirstRetryDelayInSecs = 0.01;
            })
            .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
            {
                retryPolicy1 = httpMessageHandlerBuilder
                    .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                    .FirstOrDefault();
            });
        services
            .AddHttpClient("Microsoft")
            .AddRetryPolicy(options =>
            {
                options.RetryCount = 3;
                options.MedianFirstRetryDelayInSecs = 0.02;
            })
            .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
            {
                retryPolicy2 = httpMessageHandlerBuilder
                    .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                    .FirstOrDefault();
            });

        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.InstantiateNamedHttpClient("GitHub");
        serviceProvider.InstantiateNamedHttpClient("Microsoft");

        retryPolicy1.ShouldNotBeNull();
        retryPolicy2.ShouldNotBeNull();
        ReferenceEquals(retryPolicy1, retryPolicy2).ShouldBeFalse();
        retryPolicy1.PolicyKey.ShouldNotBe(retryPolicy2.PolicyKey);
    }
}
