namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.HttpMessageHandlers;

[Trait("Category", XUnitCategories.HttpMockingHttpMessageHandlers)]
public class TestHttpMessageHandlerTests
{
    /// <summary>
    /// Validates the arguments for the <see cref="TestHttpMessageHandler.MockHttpResponse(HttpResponseMessageMock)"/> method.
    /// </summary>
    [Fact]
    public void ValidateArguments1()
    {
        using var handler = new TestHttpMessageHandler();
        var exception = Should.Throw<ArgumentNullException>(() => handler.MockHttpResponse((HttpResponseMessageMock)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'httpResponseMock')");
    }

    /// <summary>
    /// Validates the arguments for the <see cref="TestHttpMessageHandler.MockHttpResponse(Action{HttpResponseMessageMockBuilder})"/> method.
    /// </summary>
    [Fact]
    public void ValidateArguments2()
    {
        using var handler = new TestHttpMessageHandler();
        var exception = Should.Throw<ArgumentNullException>(() => handler.MockHttpResponse((Action<HttpResponseMessageMockBuilder>)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'configure')");
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> throws an exception if it gets executed
    /// but no mocks were defined.
    /// </summary>
    [Fact]
    public async Task NoMockDefined()
    {
        using var handler = new TestHttpMessageHandler();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        using var httpClient = new HttpClient(handler);
        var exception = await Should.ThrowAsync<InvalidOperationException>(httpClient.SendAsync(request));
        exception.Message.ShouldBe("No response mock defined for GET to https://test.com/.");
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> throws an exception if it gets executed
    /// but no mocks are executed because none match the HttpRequestMessage.
    /// </summary>
    [Fact]
    public async Task NoMockMatches()
    {
        using var handler = new TestHttpMessageHandler();
        handler.MockHttpResponse(builder =>
        {
            builder
                .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("microsoft", StringComparison.Ordinal))
                .RespondWith(() => new HttpResponseMessage(HttpStatusCode.OK));
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        using var httpClient = new HttpClient(handler);
        var exception = await Should.ThrowAsync<InvalidOperationException>(httpClient.SendAsync(request));
        exception.Message.ShouldBe("No response mock defined for GET to https://test.com/.");
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage.
    /// In this test no predicate is defined which means the default "always true" predicate takes effect
    /// and the mock is always returned.
    /// Using <see cref="TestHttpMessageHandler.MockHttpResponse(HttpResponseMessageMock)"/>.
    /// </summary>
    [Fact]
    public async Task DefaultPredicate1()
    {
        var httpResponseMessageMock = new HttpResponseMessageMockBuilder()
            .RespondWith(() => new HttpResponseMessage(HttpStatusCode.Created))
            .Build();
        using var handler = new TestHttpMessageHandler();
        handler.MockHttpResponse(httpResponseMessageMock);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        using var httpClient = new HttpClient(handler);
        var httpResponseMessage = await httpClient.SendAsync(request);
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage.
    /// In this test no predicate is defined which means the default "always true" predicate takes effect
    /// and the mock is always returned.
    /// Using <see cref="TestHttpMessageHandler.MockHttpResponse(Action{HttpResponseMessageMockBuilder})"/>.
    /// </summary>
    [Fact]
    public async Task DefaultPredicate2()
    {
        using var handler = new TestHttpMessageHandler();
        handler.MockHttpResponse(builder => builder.RespondWith(() => new HttpResponseMessage(HttpStatusCode.Created)));

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        using var httpClient = new HttpClient(handler);
        var httpResponseMessage = await httpClient.SendAsync(request);
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage
    /// for the FIRST match.
    /// </summary>
    [Fact]
    public async Task FirstMatchWins()
    {
        using var handler = new TestHttpMessageHandler();
        handler
            .MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("test.com", StringComparison.Ordinal))
                    .RespondWith(() => new HttpResponseMessage(HttpStatusCode.BadRequest));
            })
            .MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("test.com", StringComparison.Ordinal))
                    .RespondWith(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        using var httpClient = new HttpClient(handler);
        var httpResponseMessage = await httpClient.SendAsync(request);
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> returns the mocked HttpResponseMessage  for the appropriate predicate match.
    /// </summary>
    [Fact]
    public async Task MultipleMocks()
    {
        using var handler = new TestHttpMessageHandler();
        handler
            .MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("google.com", StringComparison.Ordinal))
                    .RespondWith(() =>
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    });
            })
            .MockHttpResponse(builder =>
            {
                builder
                    .Where(httpRequestMessage => httpRequestMessage.RequestUri!.Host.Equals("microsoft.com", StringComparison.Ordinal))
                    .RespondWith(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });

        using var httpClient = new HttpClient(handler);
        using var request1 = new HttpRequestMessage(HttpMethod.Get, "https://google.com");
        var httpResponseMessage1 = await httpClient.SendAsync(request1);
        httpResponseMessage1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        using var request2 = new HttpRequestMessage(HttpMethod.Get, "https://microsoft.com");
        var httpResponseMessage2 = await httpClient.SendAsync(request2);
        httpResponseMessage2.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Tests that the <see cref="TestHttpMessageHandler"/> times out as configured.
    /// </summary>
    [Fact]
    public async Task TimesOut()
    {
        using var handler = new TestHttpMessageHandler();
        handler.MockHttpResponse(builder => builder.TimesOut(TimeSpan.FromSeconds(2)));
        using var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(250),
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://google.com");

        // for some reason the exception returned by Should.ThrowAsync is missing the InnerException so
        // we are using the try/catch code as a workaround
        // I've raised an issue at https://github.com/shouldly/shouldly/issues/817
        try
        {
            await httpClient.SendAsync(request);
        }
        catch (TaskCanceledException exception)
        {
            exception.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            exception.ShouldBeOfType<TaskCanceledException>();
            exception.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.25 seconds elapsing.");
            exception.InnerException.ShouldBeOfType<TimeoutException>();
            exception.InnerException.Message.ShouldBe("A task was canceled.");
            return;
        }

        Assert.Fail("Expected exception but didn't get one.");
    }
}
