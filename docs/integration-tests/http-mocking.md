# Mocking HttpClient's responses

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

When doing those types of tests you need to be able to [inject mock services](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1#inject-mock-services). The docs explain how to do this. And when it comes to HttpClient calls it means you have two choices:

* Use a typed HttpClient and inject a mock for that type
* [Mock the IHttpClientFactory](http://anthonygiretti.com/2018/09/06/how-to-unit-test-a-class-that-consumes-an-httpclient-with-ihttpclientfactory-in-asp-net-core/)

### Issues with mocking the typed client

The problem with mocking the typed client is that then we won't test the implementation of the typed client. In this scenario we mock more than just the http layer. We mock the implementation of the typed client.
In the end, in addition to integration tests, we would also have to implement some unit tests for the implementation of the typed client.

### Issues with mocking the IHttpClientFactory.CreateClient

The problem with mocking the `IHttpClientFactory.CreateClient` is that any configuration that is set for the HttpClient as part of the `Startup` won't take effect. For instance, after calling `IServiceCollection.AddHttpClient` you can configure properties/behaviour of the `HttpClient` by following that call with a `IHttpClientBuilder.ConfigureHttpClient`.

Imagine that you want to configure a base address or a timeout for the `HttpClient`. If we mock the `IHttpClientFactory.CreateClient` then the call to `IHttpClientBuilder.ConfigureHttpClient` where you define the base address or a timeout won't take effect during tests because we aren't using the 'real' `IHttpClientFactory`.

As another example, if you use the [Polly library](https://github.com/App-vNext/Polly) to add resilience and transient-fault-handling to the `HttpClient` then those policies will also not take effect on your tests leaving a gap in testing.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

Once you have your test ready configure the responses of the HttpClient by using the `IWebHostBuilder.UseHttpMocks` extension method. See example DemoTest:

```
public class HttpMocksDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
	private readonly WebApplicationFactory<Startup> _webApplicationFactory;

	public HttpMocksDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
	{
		_webApplicationFactory = webApplicationFactory;
	}

	[Fact]
	public void DemoTest()
	{
		var httpResponseMock = new HttpResponseMockBuilder()
			.ForTypedClient<IMyApiClient>()
			.RespondWith(httpRequestMessage =>
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			});

		var httpClient = _webApplicationFactory
			.WithWebHostBuilder(builder =>
			{
				builder
					.ConfigureTestServices(services =>
					{
						// inject mocks for any other services
					})
					.UseHttpMocks(handlers =>
					{
						handlers.MockHttpResponse(httpResponseMock);
					});
			}).CreateClient();

		// do some calls to your app via the httpClient and then some asserts
	}
}
```

**Note**: the above test assumes that there is a typed client, represented by the type `IMyApiClient`, added to the `IServiceCollection` of the `Startup` class through the `IServiceCollection.AddHttpClient` method.

### Mock different types of HttpClients

There are [3 ways to create http clients using IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests).

When mocking the responses the way you create the mock response varies depending on the type of HttpClient.

For typed clients you need to provide the type of the client when using `HttpResponseMockBuilder.ForTypedClient`:

```
var httpResponseMock = new HttpResponseMockBuilder()
	.ForTypedClient<IMyApiClient>()
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});
```

For named clients you need to provide the name of the client when using `HttpResponseMockBuilder.ForNamedClient`:

```
var httpResponseMock = new HttpResponseMockBuilder()
	.ForNamedClient("ClientName")
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});
```

For http clients create following the Basic usage of the IHttpClientFactory use the `HttpResponseMockBuilder.ForBasicClient`:

```
var httpResponseMock = new HttpResponseMockBuilder()
	.ForBasicClient()
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});
```

### Mock responses conditionally

You can mock responses conditially by using the `HttpResponseMockBuilder.Where` method.

Imagine that you have a typed client which implemented 3 different API calls but you only wanted to mock the response for one of them. You can do:

```
var httpResponseMock = new HttpResponseMockBuilder()
	.ForTypedClient<IMyApiClient>()
	.Where(HttpRequestMessage =>
	{
		return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Users");
	})
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});
```

The above will mock http responses to the IMyApiClient typed HttpClient when the request path is */Users*.

By default, if `HttpResponseMockBuilder.Where` method is not used, it will always apply the mock.

If multiple http response mocks implement the same condition then only the response from the first mock is returned.

If you create a mock for an HttpClient and no condition is met you will receive an `HttpResponseMockBuilderException` indicating which endpoint is being called but not mocked.

### Mock several responses

You can mock several responses for the same HttpClient. For instance:

```
var usersHttpResponseMock = new HttpResponseMockBuilder()
	.ForTypedClient<IMyApiClient>()
	.Where(HttpRequestMessage =>
	{
		return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Users");
	})
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});

var adminsHttpResponseMock = new HttpResponseMockBuilder()
	.ForTypedClient<IMyApiClient>()
	.Where(HttpRequestMessage =>
	{
		return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Admins");
	})
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.BadRequest);
	});
```

and then feed the mocks to the `IWebHostBuilder.UseHttpMocks` extension method:

```
UseHttpMocks(httpMessageHandlersBuilder =>
{
	httpMessageHandlersBuilder
		.MockHttpResponse(usersHttpResponseMock)
		.MockHttpResponse(adminsHttpResponseMock);
});
```

As you would expect, you can follow the same principle and mock different responses for different HttpClients.

### Different ways to mock the HttpClient response

If you wish, instead of configuring the http response mock before hand as shown in [How to use](#how-to-use), you can configure them inline with the `IWebHostBuilder.UseHttpMocks` extension method:

```
public class HttpMocksDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
	private readonly WebApplicationFactory<Startup> _webApplicationFactory;

	public HttpMocksDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
	{
		_webApplicationFactory = webApplicationFactory;
	}

	[Fact]
	public void DemoTest()
	{
		var httpClient = _webApplicationFactory
			.WithWebHostBuilder(builder =>
			{
				builder
					.ConfigureTestServices(services =>
					{
						// inject mocks for any other services
					})
					.UseHttpMocks(httpMessageHandlersBuilder =>
					{
						httpMessageHandlersBuilder.MockHttpResponse(mockBuilder =>
						{
							mockBuilder
								.ForTypedClient<IMyApiClient>()
								.RespondWith(httpRequestMessage =>
								{
									return new HttpResponseMessage(HttpStatusCode.BadRequest);
								});
						});
					});
			}).CreateClient();

		// do some calls to your app via the httpClient and then some asserts
	}
}
```

## How to run the demo

The demo for this extension is represented by a test class.

* Go to the project `/demos/AspNetCore.Extensions.Testing.Demos/AspNetCore.Extensions.Testing.Demos.csproj`
* Run the tests for the class `HttpResponseMockingDemoTests`