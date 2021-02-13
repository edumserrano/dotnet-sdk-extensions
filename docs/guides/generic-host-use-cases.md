# Use cases for generic host

## Motivation

I want to be able to use features from asp.net apps such as IHostedService, graceful shutdown, dependency injection, logging, hostbuilder, configuration, etc, when building apps that are not asp.net apps. For instance when building a console app or an AWS Lambda function.

There are several articles explaining how to use the building blocks from the framework to get stuff like dependency injection and appsettings working. See [Using dependency injection in a .Net Core console application](https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/) and [Dependency injection (with IOptions) in Console Apps in .NET Core](https://keestalkstech.com/2018/04/dependency-injection-with-ioptions-in-console-apps-in-net-core-2/).

However if you want to get all of the benefits you get out of the box when creating an asp.net app without doing code for that yourself than you should consider using the [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

## How to use

The example below shows how to use the generic host on a console application. You'll need to install the `Microsoft.Extensions.Hosting` nuget package.

```
public class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        var myApp = host.Services.GetRequiredService<MyApp>();
        myApp.Run();

        Console.WriteLine("Press enter to terminate the app.");
        Console.ReadLine();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MyApp>();
                // add any other IServiceCollection configuration as you require
            });
}

public class MyApp
{
    public void Run()
    {
        Console.WriteLine("Hello!");
    }
}
```

Note that in this example we didn't even start the host, we just used it to get the benefits we are used to when building an asp.net app. The scenarios where you might want to consider starting/terminating the host would be when you need functionality that is attached to those operations such as:

- You want to use a [hosted service](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services): hosted services will be triggered as part of the start operation of the host and will also respond to the termination of the host.
- You want to take advantage of the host's graceful shutdown: when the host is terminating it will dispose all services added to the `IServiceCollection` as long as they implement `IDisposable` or `IAsyncDisposable`.

See the demos for more information of how to use the generic host and take advantage of other features such as configuration and logging.

## Demos

Analyse the code of the demo apps to gain a better understanding.

There's 4 demos for this:

1) [ConsoleAppWithGenericHost](/demos/guides/ConsoleAppWithGenericHost/README.md)
2) [ConsoleAppWithGenericHost2](/demos/guides/ConsoleAppWithGenericHost2/README.md)
3) [AWSLambdaWithGenericHost](/demos/guides/AWSLambdaWithGenericHost/README.md)
4) [AWSLambdaWithGenericHost2](/demos/guides/AWSLambdaWithGenericHost2/README.md)
