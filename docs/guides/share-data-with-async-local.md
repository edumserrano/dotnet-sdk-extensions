# Share data across the lifetime of an HTTP request

## Motivation

I want to share data across diferent objects during the lifetime of an HTTP request.

This could be achieved by creating a type to hold the data and registering it on the [`IServiceCollection` with a lifetime of `Scoped`](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes). This type could then be injected into the constructor of other objects and the data it holds would be on a per HTTP request basis.

This approach however does not work for all scenarios. For instance, this won't work if you want to share data per HTTP request and access it on an delegating handler that is configured for an HTTP client. In this case, due to the lifecycle of the `HttpMessageHandler` that are added to `HttpClients` the scoped service approach does not yield the expected results. Read [DI scopes in IHttpClientFactory message handlers don't work like you think they do](https://andrewlock.net/understanding-scopes-with-ihttpclientfactory-message-handlers/#httpmessagehandler-lifetime-in-ihttpclientfactory) for a more detailed explanation of this scenario and code examples.

An approach that will work in all scenarios can be achieved by using the [`AsyncLocal<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) type whose description on the documentation is as follows:

- Represents ambient data that is local to a given asynchronous control flow, such as an asynchronous method.

## How to

### Example 1

Let's take a look to how the `AsyncLocal<T>` is used to implement the [`HttpContextAccessor` class](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Http/Http/src/HttpContextAccessor.cs) which provides access to the `HttpContext` that must be unique per HTTP request.

```csharp
using System.Threading;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Provides an implementation of <see cref="IHttpContextAccessor" /> based on the current execution context. 
    /// </summary>
    public class HttpContextAccessor : IHttpContextAccessor
    {
        private static AsyncLocal<HttpContextHolder> _httpContextCurrent = new AsyncLocal<HttpContextHolder>();

        /// <inheritdoc/>
        public HttpContext? HttpContext
        {
            get
            {
                return  _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _httpContextCurrent.Value = new HttpContextHolder { Context = value };
                }
            }
        }

        private class HttpContextHolder
        {
            public HttpContext? Context;
        }
    }
}
```

This implementation contains an extra private class, the `HttpContextHolder` to make sure the `HttpContext` is clearead from all `ExecutionContexts` when it's no longer needed. Notice the use of the `AsyncLocal<HttpContextHolder>`.

The idea is that the `HttpContextAccessor` is then added to the `IServiceCollection` so that you can take a dependency on `IHttpContextAccessor` by injecting it via the constructor whenever you require access to the `HttpContext` instance.

### Example 2

Another example is on the [Header propagation middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?#header-propagation-middleware). This middleware also uses the `AsyncLocal<T>` class to enable header values to propagate as ambient data throught the execution of an HTTP request. In this scenario the headers are propagated from the incoming HTTP request into the server to the outgoing HTTP requests made by an `HttpClient`.

Let's look at the [implementation](https://github.com/dotnet/aspnetcore/tree/main/src/Middleware/HeaderPropagation/src), particularly at the part that makes use of the `AsyncLocal<T>`, which is the [HeaderPropagationValues class](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Middleware/HeaderPropagation/src/HeaderPropagationValues.cs):

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.HeaderPropagation
{
    /// <summary>
    /// Contains the outbound header values for the <see cref="HeaderPropagationMessageHandler"/>.
    /// </summary>
    public class HeaderPropagationValues
    {
        private readonly static AsyncLocal<IDictionary<string, StringValues>?> _headers = new AsyncLocal<IDictionary<string, StringValues>?>();

        /// <summary>
        /// Gets or sets the headers values collected by the <see cref="HeaderPropagationMiddleware"/> from the current request
        /// that can be propagated.
        /// </summary>
        /// <remarks>
        /// The keys of <see cref="Headers"/> correspond to <see cref="HeaderPropagationEntry.CapturedHeaderName"/>.
        /// </remarks>
        public IDictionary<string, StringValues>? Headers
        {
            get
            {
                return _headers.Value;
            }
            set
            {
                _headers.Value = value;
            }
        }
    }
}
```

Notice how the `HeaderPropagationValues` is added to the [`IServiceCollection`](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Middleware/HeaderPropagation/src/DependencyInjection/HeaderPropagationServiceCollectionExtensions.cs#L28) as a singleton.

This is done so that you can have access to the `HeaderPropagationValues` wherever you require it. For instance, when adding the [delegating handler to the `HttpClient` that provides the header propagation](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Middleware/HeaderPropagation/src/DependencyInjection/HeaderPropagationHttpClientBuilderExtensions.cs#L41).

With the above you have now set the stage so that you can set values on a instance of `HeaderPropagationValues` which will be unique per HTTP request. You can see on the [HeaderPropagationMiddleware](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Middleware/HeaderPropagation/src/HeaderPropagationMiddleware.cs) how the constructor takes in an instance of `HeaderPropagationValues` and sets the headers to be propagated which are later accessed on the [HeaderPropagationMessageHandler ](https://github.com/dotnet/aspnetcore/blob/e61245a09a5a998bf06d6054734e00e9cf068a28/src/Middleware/HeaderPropagation/src/HeaderPropagationMessageHandler.cs) which is used to set the headers to be propagated on the outgoing `HttpClient` request.

## Demos

Analyse the code of the demo app [AmbientDataDemo](/demos/guides/AmbientDataDemo/README.md) to gain a better understanding.