# Mocking HttpClient's responses out-of-process

This will allow mocking the HttpClient's response by launching an http server with predefined responses. The HttpClient(s) are then configured to send the requests to this http server. 

This is called out-of-process mocking because the mocked responses are returned by an http server which is not part of the test server running the integration test. The http calls will actually happen as opposed to the [in-process http response mocking method](./http-mocking-in-process.md).

For more information see [mocking HttpClient's responses using in-process vs out-of-process](http-mocking-in-process-vs-out-of-process.md).

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) and the scenario I want to test includes outgoing http calls made by the `HttpClient`.

When doing these types of tests you need to be able to [inject mock services](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#inject-mock-services). The docs explain how to do this. However, when it comes to inject mocks to control the behaviour of the `HttpClient(s)` used it can becomes more complicated as explained in [Mocking HttpClient's responses in-process](./http-mocking-in-process.md).

An alternative solution to mocking http calls in-process is to start an http server and make sure the `HttpClient(s)` are configured to send the calls to that server.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

After, setup the `HttpMockServer` and configure the `WebApplicationFactory` so that the `HttpClient(s)` required for the test send their requests to the `HttpMockServer` by having their base address set to the `HttpMockServer's` listening URL. See example DemoTest:

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
            .Where(httpRequest =>
            {
                return httpRequest.Path.Equals("/some-path");
            })
            .RespondWith((request, response) =>
            {
                response.StatusCode = StatusCodes.Status200OK;
            })
            .Build();

        await using var httpMockServer = new HttpMockServerBuilder()
            .UseHttpResponseMocks()
            .MockHttpResponse(httpResponseMock)
            .Build();
        var urls = await httpMockServer.StartAsync();
        var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
        var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);

        // configure the _webApplicationFactory to start your app
        // and make sure the HttpClient base address you want to test
        // is set to either the httpUrl or httpsUrl which is where 
        // the httpMockServer is listening for http requests
    }
}
```

For brevity, the above test does not show how to configure the `WebApplicationFactory` instance. For a full example see [the demos](#how-to-run-the-demo).

## Different ways to setup the `HttpMockServer`

There are two ways to setup the `HttpMockServer`:

- Based on configuring `HttpResponseMocks` via the `HttpMockServerBuilder.UseHttpResponseMocks`
- Based on a `Startup` class via the `HttpMockServerBuilder.UseStartup<T>`

### Configuring the `HttpMockServer` via `HttpMockServerBuilder.UseHttpResponseMocks`

This way let's you define the http response mocks before hand using the `HttpResponseMockBuilder`, then you use the `HttpMockServerBuilder.UseHttpResponseMocks` and set the mocks you want the server to use.

```
var httpResponseMock1 = new HttpResponseMockBuilder()
    .Where(httpRequest => httpRequest.Path.Equals("/path-a"))
    .RespondWith((request, response) => response.StatusCode = StatusCodes.Status200OK)
    .Build();
var httpResponseMock2 = new HttpResponseMockBuilder()
    .Where(httpRequest => httpRequest.Path.Equals("/path-b"))
    .RespondWith((request, response) => response.StatusCode = StatusCodes.Status200OK)
    .Build();

await using var httpMockServer = new HttpMockServerBuilder()
    .UseHttpResponseMocks()
    .MockHttpResponse(httpResponseMock1)
    .MockHttpResponse(httpResponseMock2)
    .Build();
var urls = await httpMockServer.StartAsync();
var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);
```

### Configuring the `HttpMockServer` via `HttpMockServerBuilder.UseStartup<T>`

This way let's you move the configuration of the `HttpMockServer` into a separate `Startup` class. 

Start by defining a `Startup` class as you see fit. The example below shows a very simple `Startup` where it will return `201` status code with a hello string body for any request to the `/hello` request path. Otherwise it returns a `500` status code.

```
public class MyMockStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
        app.Use(async (httpContext, next) =>
        {
            if (!httpContext.Request.Path.Equals("/hello"))
            {
                await next();
                return;
            }

            httpContext.Response.StatusCode = StatusCodes.Status201Created;
            await httpContext.Response.WriteAsync("hello");
        });
        app.Run(httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Task.CompletedTask;
        });
    }
}
```

You can configure the `Startup` class as you wish. For isntance you could even use controllers if that's what you prefer. In essence you can make the `HttpMockServer` behave just like a *real* `asp.net` application.

For an example of a mock `Startup` using controllers see [the demos](#how-to-run-the-demo).

After you have a mock `Startup` class configure the `HttpMockServer` as follows:

```
await using var httpMockServer = new HttpMockServerBuilder()
	.UseStartup<MyMockStartup>()
	.Build();
var urls = await httpMockServer.StartAsync();
var httpUrl = urls.First(x => x.Scheme == HttpScheme.Http);
var httpsUrl = urls.First(x => x.Scheme == HttpScheme.Https);
```

## `HttpMockServerBuilder.UseHostArgs` and `HttpMockServerBuilder.UseUrl`

### `HttpMockServerBuilder.UseHostArgs`

Using the `HttpMockServerBuilder.UseHostArgs` you can pass in configuration values to `HttpMockServer` just like you would to any `asp.net` app.

The `HttpMockServer's` Host is created using the `IHostBuilder.CreateDefaultBuilder`. As of now you can not extend/override the Host creation of the `HttpMockServer`.

So let's say that you want to set the environment configuration value for the `HttpMockServer`. You could do it as:

```
await using var httpMockServer = new HttpMockServerBuilder()
    .UseHostArgs("--environment", "http://*:8811;https://*:9911")
    .UseHttpResponseMocks() // or use the HttpMockServerBuilder.UseStartup<T> method
    .Build();
```

You can pass in any number of host arguments. The arguments will be concatenated and passed in to the `IHostBuilder.CreateDefaultBuilder`.

### `HttpMockServerBuilder.UseUrl`

One of the most obvious configuration values you want to define for the `HttpMockServer` is the URL where the server is listening for requests. Although you could configure that by using the `HttpMockServerBuilder.UseHostArgs`:

```
await using var httpMockServer = new HttpMockServerBuilder()
    .UseHostArgs("--urls", "http://*:8811;https://*:9911")
    .UseHttpResponseMocks() // or use the HttpMockServerBuilder.UseStartup<T> method
    .Build();
```

for convinience we also provide the the `HttpMockServerBuilder.UseUrl` method which you can use as follows:

```
await using var httpMockServer = new HttpMockServerBuilder()
    .UseUrl(HttpScheme.Http, 8811)
    .UseUrl(HttpScheme.Http, 8822)
    .UseUrl(HttpScheme.Https, 9911)
    .UseUrl(HttpScheme.Https, 9922)
    .UseHttpResponseMocks() // or use the HttpMockServerBuilder.UseStartup<T> method
    .Build();
```

The host will always be localhost but you can specify any number of http or https ports.

If the listening URL is not configured, by default a random free http port and a random free https port will be used.

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the tests for the [OutOfProcessResponseBasedDemoTests](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/OutOfProcess/ResponseBased/OutOfProcessResponseBasedDemoTests.cs) and the [OutOfProcessStartupBasedDemoTests](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/OutOfProcess/StartupBased/OutOfProcessStartupBasedDemoTests.cs) test classes.

Analyse the tests for more information on how this extension works.
