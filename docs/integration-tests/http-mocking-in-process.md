# Mocking HttpClient's responses in-process

This will allow mocking the HttpClient's response by taking control of the HttpMessageHandler(s) that is(are) used by the HttpClient(s) registered on the Startup.

The mocking of the http response happens within the test server, without any outgoing http call actually happening, and that's why this method was named in process as oposed to the [out-of-process http response mocking method](http-mocking-out-of-process.md).

For more information see [mocking HttpClient's responses using in-process vs out-of-process](http-mocking-in-process-vs-out-of-process.md).

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) and the scenario I want to test includes outgoing http calls made by the `HttpClient`.

When doing these types of tests you need to be able to [inject mock services](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#inject-mock-services). The docs explain how to do this. And when it comes to HttpClient calls it means you have two choices:

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

After, configure the responses of the HttpClient by using the `IWebHostBuilder.UseHttpMocks` extension method. See example DemoTest:

```csharp
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
                    .UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForTypedClient<IMyApiClient>()
                                .RespondWith(httpRequestMessage =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.OK);
                                });
                        });
                    });
            })
            .CreateClient();

        // do some calls to your app via the httpClient and then some asserts
    }
}
```

**Note**: the above test assumes that there is a typed client, represented by the type `IMyApiClient`, added to the `IServiceCollection` of the `Startup` class through the `IServiceCollection.AddHttpClient` method.

## Mock different types of HttpClients

There are [3 ways to create http clients using IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests).

When mocking the http responses, the way you create the mock response varies depending on the type of HttpClient registered.

For typed clients you need to provide the type of the client when using `HttpResponseMessageMockDescriptorBuilder.ForTypedClient`:

```csharp
var httpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
httpResponseMock
    .ForTypedClient<IMyApiClient>()
    .RespondWith(httpRequestMessage =>
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    });
```

For named clients you need to provide the name of the client when using `HttpResponseMessageMockDescriptorBuilder.ForNamedClient`:

```csharp
var httpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
httpResponseMock
    .ForNamedClient("ClientName")
    .RespondWith(httpRequestMessage =>
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    });
```

For http clients created following the Basic usage of the `IHttpClientFactory` use the `HttpResponseMessageMockDescriptorBuilder.ForBasicClient`:

```csharp
var httpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
httpResponseMock
    .ForBasicClient()
    .RespondWith(httpRequestMessage =>
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    });
```

## Mock responses conditionally

You can mock responses conditially by using the `HttpResponseMessageMockBuilder.Where` method.

Imagine that you have a typed client which implemented 3 different API calls but you only wanted to mock the response for one of them. You can do:

```csharp
var httpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
httpResponseMock
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

The above will mock http responses to the `IMyApiClient` typed `HttpClient` when the request path is `/Users`.

By default, if `HttpResponseMessageMockBuilder.Where` method is not used, it will always apply the mock.

If multiple http response mocks implement the same condition then only the response from the first mock is returned.

If you create a mock for an `HttpClient` and no condition is met you will receive an `InvalidOperationException` indicating which endpoint is being called but not mocked.

## Mock several responses

You can mock multiple http responses:

```csharp
var usersHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
usersHttpResponseMock
    .ForTypedClient<IMyApiClient>()
    .Where(HttpRequestMessage =>
    {
        return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Users");
    })
    .RespondWith(httpRequestMessage =>
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    });

    var adminsHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
    adminsHttpResponseMock
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

```csharp
UseHttpMocks(handlers =>
{
    handlers
        .MockHttpResponse(usersHttpResponseMock)
        .MockHttpResponse(adminsHttpResponseMock);
});
```

## Different ways to mock the HttpClient response

You might have noticed that the last example of mocking the http response is differen from the first one show in [How to use](#how-to-use). In short, you can chose to define the mocks inline or before hand.

There is no recommendation on any of the different ways to do the mocking. You should use the option that better fits your scenario/style.

Let's see some examples:

1) Configuring the http response mocks inline with the `HttpMessageHandlersReplacer.MockHttpResponse` method:

```csharp
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
                    .UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
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

2) Configuring the http response mocks before hand and using them with `IWebHostBuilder.UseHttpMocks` inline:

```csharp
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
        var usersHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
        usersHttpResponseMock
            .ForTypedClient<IMyApiClient>()
            .Where(HttpRequestMessage =>
            {
                return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Users");
            })
            .RespondWith(httpRequestMessage =>
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var adminsHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
        adminsHttpResponseMock
        .ForTypedClient<IMyApiClient>()
            .Where(HttpRequestMessage =>
            {
                return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Admins");
            })
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
                        handlers.MockHttpResponse(usersHttpResponseMock);
                        handlers.MockHttpResponse(adminsHttpResponseMock);
                    });
            }).CreateClient();

        // do some calls to your app via the httpClient and then some asserts
    }
}
```

3) Configuring the http response mocks before hand and using them with `IWebHostBuilder.UseHttpMocks` non inline:

```csharp
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
        var usersHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
        usersHttpResponseMock
            .ForTypedClient<IMyApiClient>()
            .Where(HttpRequestMessage =>
            {
                return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Users");
            })
            .RespondWith(httpRequestMessage =>
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

        var adminsHttpResponseMock = new HttpResponseMessageMockDescriptorBuilder();
        adminsHttpResponseMock
        .ForTypedClient<IMyApiClient>()
            .Where(HttpRequestMessage =>
            {
                return HttpRequestMessage.RequestUri.AbsolutePath.Equals("/Admins");
            })
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
                    .UseHttpMocks(usersHttpResponseMock,adminsHttpResponseMock);
            }).CreateClient();

        // do some calls to your app via the httpClient and then some asserts
    }
}
```

## Configure the http response mocks with access to the `IServiceProvider`

If you need to configure the http response mock based on data that depends on what is present on the `IServiceCollection` then you can use the overload that gives you access to the `IServiceProvider` to retrieve what you require. For instance:

```csharp
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
                builder.UseSetting("SomeOption", "my-option-value");
                builder
                    .ConfigureTestServices(services =>
                    {
                        // inject mocks for any other services
                    })
                    .UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse((serviceProvider, httpResponseMessageBuilder) =>
                        {
                            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                            var valueFromConfiguration = configuration.GetValue<string>("SomeOption");
                            httpResponseMessageBuilder
                                .ForTypedClient<IMyApiClient>()
                                .RespondWith(httpRequestMessage =>
                                {
                                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                                    httpResponseMessage.Headers.Add("some-header", valueFromConfiguration);
                                    return httpResponseMessage;
                                });
                        });
                    });
            }).CreateClient();

        // do some calls to your app via the httpClient and then some asserts
    }
}
```

In the above example we are retrieving the configuration value for the key `SomeOption` from the `IConfiguration` instance that we got from the `IServiceProvider` and setting it as the value of the header `some-header` for the mocked response.

The above code is just for example. In reality you will probably want to retrieve some data from the `IServiceProvider` that was added to the `IServiceCollection` by the `Startup` class that is used by the `WebApplicationFactory<Startup>`.

## How to run the demo

The demo for this extension is represented by a test class.

* In Visual Studio go to the `DotNet.Sdk.Extensions.Testing.Demos project`.
* Run the tests for the [InProcessHttpResponseMockingDemoTests class](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/InProcess/InProcessHttpResponseMockingDemoTests.cs).

Analyse the [InProcessHttpResponseMockingDemoTests class](/demos/extensions-testing-demos/DotNet.Sdk.Extensions.Testing.Demos/HttpMocking/InProcess/InProcessHttpResponseMockingDemoTests.cs) for more information on how this extension works.
