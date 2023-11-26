# Integration tests for HostedServices (Background Services)

- [Motivation](#motivation)
  - [Issues with not having a WebHost](#issues-with-not-having-a-webhost)
  - [Issues with not knowing when the Act phase of the test is done](#issues-with-not-knowing-when-the-act-phase-of-the-test-is-done)
- [Requirements](#requirements)
- [How to use](#how-to-use)
  - [Test hosted service using IHost](#test-hosted-service-using-ihost)
  - [Test hosted service using WebApplicationFactory](#test-hosted-service-using-webapplicationfactory)
- [Stop condition](#stop-condition)
- [Use a time condition to stop the test server](#use-a-time-condition-to-stop-the-test-server)
- [Configure a timeout for the condition set to stop the test server](#configure-a-timeout-for-the-condition-set-to-stop-the-test-server)
- [Configuring the interval of time on which the condition is checked](#configuring-the-interval-of-time-on-which-the-condition-is-checked)


This extension allows you to do integration tests for your Background Services.

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) but for scenarios that make use of [Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services).

When trying to do this you face 2 issues:

* The integration tests docs are made for web apps. If you just create a Background Service app and don't use a WebHost then there is no equivalent of `WebApplicationFactory` for doing integration tests against your host.
* Using [AAA terminology](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics), how do you know when your act is done so that you can do your asserts ?

### Issues with not having a WebHost

At the moment, the solution for this is to change your Host to a WebHost. If you don't do this at the moment you can't use the process described below for doing integration tests on Hosted Services.

By default on the Worker Service template, the `IHost` instance is created as follows:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<Worker>();
        });
```

To be able to use this testing extension you should change it to:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

And on the `ConfigureServices method` of the `Startup` class is where you add any Hosted Services you require:

```csharp
public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<Worker>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // add here any IApplicationBuilder configuration if required
    }
}
```

### Issues with not knowing when the Act phase of the test is done

The problem is that you only want to do your asserts when the Hosted Service has finished its work for your given test scenario. With this in mind, your basic test layout would be:

* Configure any mocks required and inject them in the test server
* Start the test server
* Wait for the Hosted Service to complete it's work for the given test case
* Stop the test server
* Do the asserts

Out of the box there is no way for you to know when the Hosted Service has finished the work. The simplistic solution is to always wait for a given period of time and then do the asserts. This is not the best solution because the wait times depend on the hardware on which the tests are run and usually leads to flaky tests.

The provided solution will let you do this on a custom condition or as well as on time based condition if that's what you actually require.

## Requirements

You will have to add the [dotnet-sdk-extensions-testing](https://www.nuget.org/packages/dotnet-sdk-extensions-testing) nuget to your test project.

## How to use

The [dotnet-sdk-extensions-testing](https://www.nuget.org/packages/dotnet-sdk-extensions-testing) contains an `IHost.RunUntilAsync` and a `WebApplicationFactory.RunUntilAsync` extension methods. The `RunUntilAsync` executes the host until a condition has been met:

* If you want to test a hosted service on a project which does not start a web server, like when using the `Worker Service` template, then follow the instructions at [Test hosted service using IHost](#test-hosted-service-using-ihost).
* If you want to test a hosted service on a project which starts a web server, like when using the `ASP.NET Core Web API` template, then follow the instructions at [Test hosted service using WebApplicationFactory](#test-hosted-service-using-webapplicationfactory).

### Test hosted service using IHost

When creating a project using the `Worker Service` template, the `IHost` instance is created and executed as follows:

```csharp
IHost host = Host.CreateDefaultBuilder(args)
   .ConfigureServices(services =>
   {
       services.AddHostedService<Worker>();
   })
   .Build();
host.Run();
```

To run an integration test using the `IHost` we need to create a wrapper around the `IHostBuilder` so that it's shared between your app and the tests and so that we can mock dependencies if needed.

You can do this however you like, the following shows a simple approach:

1) Create a wrapper class that exposes the `IHostBuilder`.

```csharp
public class WorkerHostBuilder
{
    public WorkerHostBuilder(params string[] args)
    {
        Builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            });
    }

    public IHostBuilder Builder { get; }
}
```

2) Change your `Program.cs` to use the wrapper class.

```csharp
var workerHostBuilder = new WorkerHostBuilder(args);
var host = workerHostBuilder.Builder.Build();
host.Run();
```

3) Create a test where you use the wrapper class to build the `IHost`.

```csharp
[Fact]
public async Task DemoTest()
{
    var workerHostBuilder = new WorkerHostBuilder()
        .Builder
        .ConfigureServices(services =>
        {
            services.AddSingleton<ICalculator>(calculatorMock);
        });
    var host = workerHostBuilder.Build();
    await host.RunUntilAsync(() => <some condition>);

    // do some asserts
}
```

The above are the basic steps to do a test. Let's imagine that our Hosted Service will be modified to do the following:

```csharp
public interface ICalculator
{
    int Sum(int left, int right);
}

public class Calculator : ICalculator
{
    public int Sum(int left, int right)
    {
        return left + right;
    }
}

public class Worker : BackgroundService
{
    private readonly ICalculator _calculator;

    public Worker(ICalculator calculator)
    {
        this._calculator = calculator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _calculator.Sum(1, 1);
            await Task.Delay(300, stoppingToken);
        }
    }
}
```

The difference from the default `Worker Service` template is that we added a dependency of type `ICalculator` and made the Hosted Service invoke the `ICalculator.Sum` method.

Now let's say that we want to make a test to the Hosted Service. We could say for instance that we want to run our test until the `ICalculator.Sum` method was called 3 times and then do some asserts. We can do that as follows:

1) Make sure the `ICalculator` dependency is added to the wrapper class that exposes the `IHostBuilder`. Without this the host service would fail to run because it would be able to provide an instance of `ICalculator` to the Hosted Service.

```csharp
public class WorkerHostBuilder
{
    public WorkerHostBuilder(params string[] args)
    {
        Builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
                services.AddSingleton<ICalculator, Calculator>();
            });
    }

    public IHostBuilder Builder { get; }
}
```

2) Override the `ICalculator` dependency so that we can use it as a condition for stopping our test when the `ICalculator.Sum` is called 3 times.

```csharp
[Fact]
public async Task DemoTest2()
{
    var callCount = 0;
    var calculatorMock = Substitute.For<ICalculator>(); // using NSubstitute for mocking but you can use whatever you prefer
    calculatorMock
        .Sum(Arg.Any<int>(), Arg.Any<int>())
        .Returns(1)
        .AndDoes(info => callCount++); // keep count of how many times the ICalculator.Sum method has been called
    var workerHostBuilder = new WorkerHostBuilder()
        .Builder
        .ConfigureServices(services =>
        {
            services.AddSingleton<ICalculator>(calculatorMock);
        });
    var host = workerHostBuilder.Build();
    await host.RunUntilAsync(() => callCount >= 3); // stop host execution when the ICalculator.Sum method has been called 3 times

    // do some asserts
}
```

The above is a very simple example that hopefully gives you an idea on how you can do the integratin style tests for Hosted Services.

> [!NOTE]
>
> You can also consider converting your project to start a web application and follow the instructions at [Test hosted service using WebApplicationFactory](#test-hosted-service-using-webapplicationfactory).
>
> To convert to a project that starts a web application use a template like the `ASP.NET Core Web API` and migrate your code accross. You can take manual steps to convert a project that doesn't start a web application, like one that uses a `Worker Service` template, but it's probably easier to do it by creating a new project and moving the code across to it.
>

### Test hosted service using WebApplicationFactory

If you already have a project which starts a web application, like the one you get from using the `ASP.NET Core Web API` template, then you should start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

The type of the generic used in `WebApplicationFactory<T>` needs to be a class from your project. You can follow the [documentation on the official examples](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory) to expose the `Program` type so that it can be used in `WebApplicationFactory<Program>` or you can make the Hosted Service `public` and use that as the generic type on `WebApplicationFactory<T>`. The example below uses the latter.

For demo purposes let's assume that the Hosted Service is going to execute the method `ICalculator.Sum` on a loop. Let's also say that we want to make a test to the Hosted Service where we want to run the test until the `ICalculator.Sum` method was called 3 times and then do some asserts. We can do that as follows:

1) Declare the `ICalculator` type and a type that implements it.

```csharp
public interface ICalculator
{
    int Sum(int left, int right);
}

public class Calculator : ICalculator
{
    public int Sum(int left, int right)
    {
        return left + right;
    }
}
```

2) Register the `ICalculator` type in the service collection. On the `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Worker>(); // the hosted service
builder.Services.AddSingleton<ICalculator, Calculator>(); // register the ICalculator type
```

3) Update the Hosted Service to call the `ICalculator.Sum`:

```csharp
public class Worker : BackgroundService
{
    private readonly ICalculator _calculator;

    public Worker(ICalculator calculator)
    {
        this._calculator = calculator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _calculator.Sum(1, 1);
            await Task.Delay(300, stoppingToken);
        }
    }
}
```

4) Update the test to mock the `ICalculator` type and set a condition to stop the host when the `ICalculator.Sum` has been called 3 times:

```csharp
public class HostedServiceDemoTests : IClassFixture<WebApplicationFactory<Worker>>
{
    private readonly WebApplicationFactory<Worker> _webApplicationFactory;

    public HostedServiceDemoTests(WebApplicationFactory<Worker> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task DemoTest()
    {
        var callCount = 0;
        var calculatorMock = Substitute.For<ICalculator>(); // using NSubstitute for mocking but you can use whatever you prefer
        calculatorMock
            .Sum(Arg.Any<int>(), Arg.Any<int>())
            .Returns(1)
            .AndDoes(info => callCount++); // keep count of how many times the ICalculator.Sum method has been called

        await _webApplicationFactory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<ICalculator>(calculatorMock);
                });
            })
            .RunUntilAsync(() => callCount >= 3); // stop host execution when the ICalculator.Sum method has been called 3 times

        // do some asserts
    }
}
```

The above is a very simple example that hopefully gives you an idea on how you can do the integratin style tests for Hosted Services.

## Stop condition

The main difference from the integration test examples shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) is that you do not use the `WebApplicationFactory.CreateClient()` and then use the returned HttpClient do to calls into the test server but instead you use the `WebApplicationFactory.RunUntilAsync` extension method with a custom conditions that will control the lifetime of the test server when using Hosted Services.

> [!NOTE]
>
> When thinking about your test scenario understand that your code running on your Hosted Service won't immediatly stop when the test condition is reached. In reality, the set condition is checked periodically to understand if the test server should be stopped.
>

To put it another way, the set condition actually means *don't stop the server before at least this condition is met*.

This is important when planning your stop condition and asserts as it might mean that more of your code executed than you might initially think if you don't plan your stop condition appropriately.

 As an example if your Hosted Service is in a while loop doing some operation and you are keeping count of how many times that operation has run before stopping the test server, then the stop condition should probably be `numberOfRuns >= 'some value'` instead of `numberOfRuns == 'some value'`.

## Use a time condition to stop the test server

If you prefer to run the web server for a period of time before terminating it you can use the `WebApplicationFactory.RunUntilTimeoutAsync` extension method:

Given that you have an instance of `WebApplicationFactory` you can do someting like:

```csharp
await _webApplicationFactory
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            // inject mocks for any other services
        });
    })
    .RunUntilTimeoutAsync(TimeSpan.FromSeconds(3));
```

Usually it's best to consider stopping after a stop condition is met. Abusing the `WebApplicationFactory.RunUntilTimeoutAsync` and using it in scenarios where you could have set a stop condition using the `WebApplicationFactory.RunUntilAsync` might lead to flaky tests.

## Configure a timeout for the condition set to stop the test server

When setting a condition to for the `WebApplicationFactory.RunUntilAsync` extension method there is a default timeout of 5 seconds set to reach that condition. If the condition is not reached the test server is stopped and a `RunUntilException` is thrown.

This is to avoid having a test that never ends because the set condition is never reached. You can configure this timeout by using an overload of `WebApplicationFactory.RunUntilAsync` as follows:

```csharp
await _webApplicationFactory
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ICalculator>(someMock);
        });
    })
    .RunUntilAsync(() => callCount == 3, options => options.Timeout = TimeSpan.FromMilliseconds(100));
```

The above changes the default 5 seconds timeout to 100 milliseconds.

**Note that when debugging** (`Debugger.IsAttached` is true) the default timeout will not be 5 seconds, it will instead be 1 day. This is done so that you can take your time when debugging tests and not have the timeout being triggered and abort the test server in the middle of debugging.

The above is only true for the default timeout. Meaning that any timeout that you set is honored **even when debugging**.

Beware of this when you're debugging tests where you've set a low timeout. You might have to increase your set timeout to something large enough to let you debug your test properly and then once you're happy set it back to the desired timeout.

## Configuring the interval of time on which the condition is checked

When you set a condition, that condition is checked in a loop until it's reached or until the timeout is triggered:

* if it evaluates to true the server is stopped
* if it evaluates to false the condition is only checked after some time

By default the condition is checked in intervals of 5 milliseconds. This can be configured by using the an overload of `WebApplicationFactory.RunUntilAsync` extension method as follows:

```csharp
await _webApplicationFactory
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ICalculator>(someMock);
        });
    })
    .RunUntilAsync(() => callCount == 3, options => options.PredicateCheckInterval = TimeSpan.FromMilliseconds(100));
```

Setting the `RunUntilOptions.PredicateCheckInterval` to high values might mean your test takes longer to finish because one way one thinking about this setting is that it represents the longest time possible between your condition evaluating to true and the server being given the order to stop.

So if for your test it will take X time to meet the condition and the `RunUntilOptions.PredicateCheckInterval` is represented by Y than in the worst case scenario the time to run your test will be close to X + Y.

[!NOTE]: when debugging it might be useful to set the `RunUntilOptions.PredicateCheckInterval` to a larger period to allow you to step through your code more easily before the check for the condition kicks in and, if evaluates to true, shuts down the test server and ends the test.
