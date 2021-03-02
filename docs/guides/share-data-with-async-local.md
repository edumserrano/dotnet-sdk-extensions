# Share data across the lifetime of an HTTP request

## Motivation

I want to share data across diferent objects during the lifetime of an HTTP request.

This could be achieved by creating a type to hold the data and registering it on the [`IServiceCollection` with a lifetime of `Scoped`](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes). This type could then be injected into the constructor of other objects and the data it holds would be on a per HTTP request basis.

This approach however does not work for all scenarios. For instance, this won't work if you want to share data per HTTP request and access it on an delegating handler that is configured for an HTTP client. In this case, due to the lifecycle of the `HttpMessageHandler` that are added to `HttpClients` the scoped service approach does not yield the expected results. Read [DI scopes in IHttpClientFactory message handlers don't work like you think they do](https://andrewlock.net/understanding-scopes-with-ihttpclientfactory-message-handlers/#httpmessagehandler-lifetime-in-ihttpclientfactory) for a more detailed explanation of this scenario and code examples.

If you want an approach that will work in all scenarios then you should consider using the [`AsyncLocal<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) type.

## How to

Let's take a look to how the `AsyncLocal<T>` is used to implement the `HttpContext` class.
//https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http/src/HttpContextAccessor.cs

[here](https://github.com/dotnet/aspnetcore/blob/262369301aa24bb7626e41b88cce915daeb7827a/src/Http/Http/src/HttpContextAccessor.cs#L1-L45)
