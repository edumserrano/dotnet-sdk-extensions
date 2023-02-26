namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess;

[Trait("Category", XUnitCategories.HttpMockingInProcess)]
public sealed class TimeoutTests : IClassFixture<TimeoutHttpResponseMockingWebApplicationFactory>
{
    private readonly TimeoutHttpResponseMockingWebApplicationFactory _webApplicationFactory;

    public TimeoutTests(TimeoutHttpResponseMockingWebApplicationFactory webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    /// <summary>
    /// The setup for this test sets up a named HttpClient "named-client-with-low-timeout" and with a configured timeout of 250ms,
    /// see <see cref="TimeoutStartupHttpResponseMocking"/> for more info on the setup.
    ///
    /// This tests that if we define a mock to timeout then it will timeout as long as the mock timeout is higher than
    /// the <see cref="HttpClient.Timeout"/>.
    ///
    /// In this test the timeout of 2 second defined on the mock is higher than the timeout of 250ms defined on the HttpClient.
    /// </summary>
    [Fact]
    public async Task TimeoutOnHttpClientWithTimeoutConfigured1()
    {
#if NET6_0 || NET7_0
        await using var webAppFactory = _webApplicationFactory
#else
        using var webAppFactory = _webApplicationFactory
#endif
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseHttpMocks(handlers =>
                {
                    handlers.MockHttpResponse(httpResponseMessageBuilder =>
                    {
                        httpResponseMessageBuilder
                            .ForNamedClient("named-client-with-low-timeout")
                            .TimesOut(TimeSpan.FromSeconds(2));
                    });
                });
            });

        var httpClient = webAppFactory.CreateClient();
        await httpClient.GetAsync("/named-client-with-low-timeout");

        var exceptionService = webAppFactory.Services.GetRequiredService<ExceptionService>();
        var expectedException = exceptionService.Exceptions.FirstOrDefault();
        expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
        expectedException.ShouldBeOfType<TaskCanceledException>();
#if NETCOREAPP3_1
        expectedException.Message.ShouldBe("A task was canceled.");
        expectedException.InnerException.ShouldBeNull();
#else
        expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.25 seconds elapsing.");
        expectedException.InnerException.ShouldBeOfType<TimeoutException>();
        expectedException.InnerException.Message.ShouldBe("A task was canceled.");
#endif
    }

    /// <summary>
    /// The setup for this test sets up a named HttpClient "named-client-with-high-timeout" and with a configured timeout of 100s,
    /// see <see cref="TimeoutStartupHttpResponseMocking"/> for more info on the setup.
    ///
    /// This tests that if we define a mock to timeout with a lower value than the than the <see cref="HttpClient.Timeout"/>
    /// then we do NOT get the timeout exception.
    ///
    /// In this test the timeout of 50ms second defined on the mock is lower than the timeout of 100s defined on the HttpClient.
    /// </summary>
    [Fact]
    public async Task TimeoutOnHttpClientWithTimeoutConfigured2()
    {
#if NET6_0 || NET7_0
        await using var webAppFactory = _webApplicationFactory
#else
        using var webAppFactory = _webApplicationFactory
#endif
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseHttpMocks(handlers =>
                {
                    handlers.MockHttpResponse(httpResponseMessageBuilder =>
                    {
                        httpResponseMessageBuilder
                            .ForNamedClient("named-client-with-high-timeout")
                            .TimesOut(TimeSpan.FromMilliseconds(50));
                    });
                });
            });

        var httpClient = webAppFactory.CreateClient();
        await httpClient.GetAsync("/named-client-with-high-timeout");

        var exceptionService = webAppFactory.Services.GetRequiredService<ExceptionService>();
        var expectedException = exceptionService.Exceptions.FirstOrDefault();
        expectedException.ShouldNotBeNull("Expected InvalidOperationException but didn't get any.");
        expectedException.ShouldBeOfType<TimeoutExpectedException>();
        expectedException.Message.ShouldBe("The request should have been aborted but it wasn't. Make sure the HttpClient.Timeout value is set to a value lower than 0.05 seconds.");
    }

    /// <summary>
    /// The setup for this test uses Polly to define a timeout policy for the named HttpClient "polly-named-client-with-low-timeout".
    /// The timeout for the policy is set to 250ms and the HttpClient is invoked when doing a GET to /polly-named-client-with-low-timeout.
    ///
    /// This tests that if we define a mock to timeout then it will timeout as long as the mock timeout is higher than the timeout on the
    /// Polly policy.
    ///
    /// In this test the timeout of 2 second defined on the mock is higher than the timeout of 250ms defined by the Polly policy
    /// on the HttpClient so Polly throws a TimeoutRejectedException when a timeout occurs.
    /// </summary>
    [Fact]
    public async Task TimeoutWithPolly1()
    {
#if NET6_0 || NET7_0
        await using var webAppFactory = _webApplicationFactory
#else
        using var webAppFactory = _webApplicationFactory
#endif
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseHttpMocks(handlers =>
                {
                    handlers.MockHttpResponse(httpResponseMessageBuilder =>
                    {
                        httpResponseMessageBuilder
                            .ForNamedClient("polly-named-client-with-low-timeout")
                            .TimesOut(TimeSpan.FromSeconds(2));
                    });
                });
            });

        var httpClient = webAppFactory.CreateClient();
        await httpClient.GetAsync("/polly-named-client-with-low-timeout");

        var exceptionService = webAppFactory.Services.GetRequiredService<ExceptionService>();
        var expectedException = exceptionService.Exceptions.FirstOrDefault();
        expectedException.ShouldNotBeNull("Expected TimeoutRejectedException but didn't get any.");
        expectedException.ShouldBeOfType<TimeoutRejectedException>();
        expectedException.Message.ShouldBe("The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.");
    }

    /// <summary>
    /// The setup for this test uses Polly to define a timeout policy for the named HttpClient "polly-named-client-with-high-timeout".
    /// The timeout for policy is set to 100s and the HttpClient is invoked when doing a GET to /polly-named-client-with-high-timeout.
    ///
    /// This tests that if we define a mock to timeout with a lower value than the than the timeout defined by the Polly policy
    /// then we do NOT get the timeout exception.
    ///
    /// In this test the timeout of 50ms defined on the mock is lower than the timeout of 100s defined by the Polly policy.
    /// </summary>
    [Fact]
    public async Task TimeoutWithPolly2()
    {
#if NET6_0 || NET7_0
        await using var webAppFactory = _webApplicationFactory
#else
        using var webAppFactory = _webApplicationFactory
#endif
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseHttpMocks(handlers =>
                {
                    handlers.MockHttpResponse(httpResponseMessageBuilder =>
                    {
                        httpResponseMessageBuilder
                            .ForNamedClient("polly-named-client-with-high-timeout")
                            .TimesOut(TimeSpan.FromMilliseconds(50));
                    });
                });
            });

        var httpClient = webAppFactory.CreateClient();
        await httpClient.GetAsync("/polly-named-client-with-high-timeout");

        var exceptionService = webAppFactory.Services.GetRequiredService<ExceptionService>();
        var expectedException = exceptionService.Exceptions.FirstOrDefault();
        expectedException.ShouldNotBeNull("Expected InvalidOperationException but didn't get any.");
        expectedException.ShouldBeOfType<TimeoutExpectedException>();
        expectedException.Message.ShouldBe("The request should have been aborted but it wasn't. Make sure the HttpClient.Timeout value is set to a value lower than 0.05 seconds.");
    }
}
