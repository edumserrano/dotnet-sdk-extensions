# Notes on WebApplicationFactory\<T>

## Motivation

There are some details when using [WebApplicationFactory\<T>](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#basic-tests-with-the-default-webapplicationfactory) that are a bit obscure but require understanding when you want to use it slightly differently from it's basic use case.

## Basic use case

The basic use case is that you create a test project and use WebApplicationFactory\<T> where T is defined in the project that contains your app (project to test). This also means that the assembly where the type T resides will by default, because of the project template, have a a class, by default the Program class, with the following method defined:

`public static IHostBuilder CreateHostBuilder(string[] args)`

In the basic use case when you use WebApplicationFactory\<T> **what happens is** that the assembly of the type T is scanned with to find a method with one of the following signatures:

- `public static IHostBuilder CreateHostBuilder(string[] args)`
- `public static IWebHostBuilder CreateWebHostBuilder(string[] args)`

If that method is not found the WebApplicationFactory\<T> will throw an exception. If found then it will use that method to create the Host. Because the startup class to be used is defined in that CreateHostBuilder/CreateWebHostBuilder method, usually via a call to [WebBuilder.UseStartup](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.webhostbuilderextensions.usestartup), then that's the Startup type that ends up being used.

Hopefully this explains properly how the Startup type is chosen: **the Startup type is NOT the type T specified on the WebApplicationFactory\<T> but rather the one defined when configuring the Host. The type T on the WebApplicationFactory\<T> is used to signal the assembly which will be scanned to find by convention how to create a Host.**

This is hinted briefly [on the docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#basic-tests-with-the-default-webapplicationfactory). Note the definition for TEntryPoint: "WebApplicationFactory<TEntryPoint> is used to create a TestServer for the integration tests. TEntryPoint is the entry point class of the SUT, usually the Startup class.".

## Problem with convention based implementation of WebApplicationFactory\<T>

For me the biggest problem is lack of documentation on how WebApplicationFactory\<T> works.

As soon as you need to step outside the [basic use case](#basic-use-case) you start running into problems. The ones I faced were around:

- How to define the startup class to be used ? This is specially relevant if you have more than one `Startup` type class in the same assembly.
- Issues caused by how the content root is specified by default.

To deal with the points above you have to implement a [custom WebApplicationFactory\<T>](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#customize-webapplicationfactory) and override some of it's methods:

- `protected override IHostBuilder CreateHostBuilder()`
- `protected override void ConfigureWebHost(IWebHostBuilder builder)`

Which method(s) you need to override and how to implement them depends on your use case. However some things to note:

- Override `IHostBuilder CreateHostBuilder()` if you do not want or can't rely on the convention based discovery process of how to create an instance of IHost. `IWebHostBuilder CreateWebHostBuilder()` remains for backwards compatibility and provides a way to define how to create an instance of IWebHost.
- Override `void ConfigureWebHost(IWebHostBuilder builder)` if you need to configure the IWebHostBuilder instance.

### Example

To bring this together I will show an example implementation of a custom WebApplicationFactory\<T>, the scenario for which I required it and what problems I encountered.

My scenario was having a test project where I had several test web app scenarios, each one with it's own Startup type. In my test project I now need to implement integration tests using WebApplicationFactory\<T> for each of the Startup types that I have.

My first attempt was to use the WebApplicationFactory\<T> where the type T would be the type of the Startup type I wanted to use in the integration test. Well, this doesn't work because as [explained above](#basic-use-case) the type T is a type to define the assembly to be scanned and find by convention how the Host is to be created. In my scenario my test project does not have the expected convention based methods, nor should it have.

So what I have to do is create a custom WebApplicationFactory\<T> as such:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<SomeTypeInMyTestsProject>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<OneOfMyTestStartupTypes>();
            });
    }
}
```

Now we are a bit better but we start having another issue. The problem is [how the content root is defined by default](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#how-the-test-infrastructure-infers-the-app-content-root-path). As explained in the docs, since I do not have a [`WebApplicationFactoryContentRootAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactorycontentrootattribute) defined what happens is that the content root is set to `Solution Directory\Assembly Name` directory. For my case this makes the WebApplicationFactory\<T> throw an exception because the folder structure for my repository didn't match the default convention. It doesn't work because my test project is inside a `tests` folder. So I get a directory not found exception because in my case `Solution Directory\Assembly Name` doesn't exist, what does exist is `Solution Directory\tests\Assembly Name`.

Ok, so how do we resolve this. One way would be by setting the content root on the `ConfigureWebHost` method:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<SomeTypeInMyTestsProject>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<OneOfMyTestStartupTypes>();
            });
    }
}
```

**With this now everything works as expected**.

As a side note moving the `UseContentRoot` to the `CreateHostBuilder` does NOT work:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<SomeTypeInMyTestsProject>
{
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseContentRoot(".");
                webBuilder.UseStartup<OneOfMyTestStartupTypes>();
            });
    }
}
```

I don't know why and I didn't research further.

However moving the `UseStartup` to `ConfigureWebHost` still produces the expected outcome:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<SomeTypeInMyTestsProject>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
        builder.UseStartup<OneOfMyTestStartupTypes>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {

            });
    }
}
```

**Note**: the call to `ConfigureWebHostDefaults` in the method `CreateHostBuilder` is likely required if you're testing web apps because it will register default services usually required by web apps. For example: `IServiceCollection.AddRouting`.
