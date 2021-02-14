# Mocking HttpClient's responses for unit testing

This will allow mocking the HttpClient's response by taking replacing the HttpMessageHandler used by the HttpClient.

This is useful when doing unit tests on classes that take in an `HttpClient` as a dependency.

## Motivation

I want to be able to do unit tests on classes that use an `HttpClient` and I need to mock the calls from the `HttpClient` to be able to do tests without depending on real services being up and so that I can have full control on the responses returned by the `HttpClient` as to allow me to test a variety of scenarios.

Although you could create another layer of abstraction over the `HttpClient` and mock that abstraction in your unit tests, that would make it harder to integrate with the .net's guidance on [how to use `HttpClient`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0). Furthermore, it would also mean more code to do and mantain.

Another solution is to take control of the `HttpMessageHandler` used by the `HttpClient` so that you can chose how the `HttpClient` responds.  The rest of this document will show an implementation of this.

## How to use

Let's assume that the class that you want to unit test is a class called `MyAwesomeOutboundDependency`. The `MyAwesomeOutboundDependency` takes in an `HttpClient` as a dependency on its constructor and then uses that `HttpClient` to make some http calls.

For us to unit test the `MyAwesomeOutboundDependency` we now need to be able to control the responses from the `HttpClient`. We can do that as follows:

```
public class HttpClientMocksDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
	[Fact]
	public void DemoTest()
	{
		// prepare the http mocks
		var httpResponseMessageMock = new HttpResponseMessageMockBuilder()
			.Where(httpRequestMessage =>
			{
				return httpRequestMessage.Method == HttpMethod.Get &&
					httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call");
			})
			.RespondWith(httpRequestMessage =>
			{
				var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
				httpResponseMessage.Content = new StringContent("mocked value");
				return httpResponseMessage;
			})
			.Build();

		// add the mocks to the http handler
		var handler = new TestHttpMessageHandler();
		handler.MockHttpResponse(httpResponseMessageMock);

		// instantiate the http client with the test handler
		var httpClient = new HttpClient(handler);
		var sut = new MyAwesomeOutboundDependency(httpClient);

		// in this example the sut.DoSomeHttpCall method call will do a GET request to the path /some-http-call
		// so it will match our mock conditions defined above and the mock response will be returned
		var response = await sut.DoSomeHttpCall(); 
		response.StatusCode.ShouldBe(HttpStatusCode.Created);
		var responseBody = await response.Content.ReadAsStringAsync();
		responseBody.ShouldBe("mocked value");
	}
}
```

## Mock several responses

You can mock multiple http responses:

```
// prepare the http mocks
var someHttpCallMock = new HttpResponseMessageMockBuilder()
	.Where(httpRequestMessage => httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call"))
	.RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.Created)
	{
		Content = new StringContent("some mocked value")
	})
	.Build();
var anotherHttpCallMock = new HttpResponseMessageMockBuilder()
	.Where(httpRequestMessage => httpRequestMessage.RequestUri.PathAndQuery.Equals("/another-http-call"))
	.RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.Accepted)
	{
		Content = new StringContent("another mocked value")
	})
	.Build();

// add the mocks to the http handler
var handler = new TestHttpMessageHandler();
handler
	.MockHttpResponse(someHttpCallMock)
	.MockHttpResponse(anotherHttpCallMock);
```

## Different ways to mock the HttpClient response

You can mock the http responses before hand or inline with the `TestHttpMessageHandler.MockHttpResponse` method.

Mocking the responses before hand looks like this:

```
var someHttpCallMock = new HttpResponseMessageMockBuilder()
	.Where(httpRequestMessage => httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call"))
	.RespondWith(httpRequestMessage => new HttpResponseMessage(HttpStatusCode.Created)
	{
		Content = new StringContent("some mocked value")
	})
	.Build();

var handler = new TestHttpMessageHandler();
handler.MockHttpResponse(someHttpCallMock);
```

Mocking the responses inline looks like this:

```
var handler = new TestHttpMessageHandler();
handler.MockHttpResponse(builder =>
{
	builder.Where(httpRequestMessage => 
	{
		return httpRequestMessage.Method == HttpMethod.Get &&
				httpRequestMessage.RequestUri.PathAndQuery.Equals("/some-http-call");
	})
	.RespondWith(httpRequestMessage =>
	{
		var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
		httpResponseMessage.Content = new StringContent("mocked value");
		return httpResponseMessage;
	});
});
```

There is no recommendation on any of the different ways to do the mocking. You should use the option that better fits your scenario/style.

## Timeouts

You can also test timeouts by configuring the http response mock using the `HttpResponseMessageMockBuilder.TimesOut` instead of the `HttpResponseMessageMockBuilder.RespondsWith` method as such:

```
var handler = new TestHttpMessageHandler();
handler.MockHttpResponse(builder => builder.TimesOut(TimeSpan.FromMilliseconds(1)));

// instantiate the http client with the test handler
var httpClient = new HttpClient(handler);

// show that the http call will timeout
Exception? expectedException = null;
try
{
	await httpClient.GetAsync("/");
}
catch (Exception exception)
{
	expectedException = exception;
}

// show that you get the expected timeout exception
expectedException!.GetType().ShouldBe(typeof(TaskCanceledException));
expectedException.InnerException!.GetType().ShouldBe(typeof(TimeoutException));
expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.001 seconds elapsing.");
expectedException.InnerException.Message.ShouldBe("A task was canceled.");
```


## Notes

* When no mock is configured on the `TestHttpMessageHandler` or when no configured mock matches the incoming request,  the `HttpClient` will throw an `InvalidOperationException` with information about the request being made which lacks mocking.
  
* When no predicate is defined on the http response mock via the `HttpResponseMessageMockBuilder.Where` method, the default predicate is an *always true* which means the mock will always match the incoming request. In code, the below two http mocks are equal:

Explicit predicate with the `HttpResponseMessageMockBuilder.Where` method:

```
var handler = new TestHttpMessageHandler();
handler.MockHttpResponse(builder =>
{
	builder.Where(httpRequestMessage => 
	{
		return true;
	})
	.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.Created);
	});
});
```

Default predicate without the `HttpResponseMessageMockBuilder.Where` method:

```
var handler = new TestHttpMessageHandler();
handler.MockHttpResponse(builder =>
{
	builder.RespondWith(httpRequestMessage =>
	{
		return new HttpResponseMessage(HttpStatusCode.Created);
	});
});
```

* When two or more mocks have a where clause that will match for the same request, the first mock added to the `TestHttpMessageHandler` is the one that takes effect. For instance:

```
var handler = new TestHttpMessageHandler()
	.MockHttpResponse(builder =>
	{
		builder
			.Where(httpRequestMessage => httpRequestMessage.RequestUri.Host.Equals("test.com"))
			.RespondWith(new HttpResponseMessage(HttpStatusCode.BadRequest));
	})
	.MockHttpResponse(builder =>
	{
		builder
			.Where(httpRequestMessage => httpRequestMessage.RequestUri.Host.Equals("test.com"))
			.RespondWith(new HttpResponseMessage(HttpStatusCode.InternalServerError));
	});

var httpClient = new HttpClient(handler);
var httpResponseMessage = await httpClient.GetAsync("https://test.com");
/*
* the httpResponseMessage.StatusCode property will be HttpStatusCode.BadRequest
* and not HttpStatusCode.InternalServerError because although both mocks match 
* the predicate specified on the HttpResponseMessageMockBuilder.Where method
* only the first mock is executed
*/
```

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the tests for the [TestHttpMessageHandlerDemoTests class](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/HttpMessageHandlers/TestHttpMessageHandlerDemoTests.cs).

Analyse the [TestHttpMessageHandlerDemoTests class](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/HttpMessageHandlers/TestHttpMessageHandlerDemoTests.cs) for more information on how this extension works.
