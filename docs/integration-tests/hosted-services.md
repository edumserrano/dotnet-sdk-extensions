# Integration tests for HostedServices

## Motivation

I want to be able to do integration tests as defined in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) but for scenarios that make use of [Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services).

When trying to do this you face 2 issues:

* The integration tests docs are made for web apps. If you just create a Background Service app and don't use a WebHost then there is no equivalent of `WebApplicationFactory` for doing integration tests against your host.
* Using [AAA terminology](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics), how do you know when your act is done so that you can do your asserts ?

### Issues with not having a WebHost

At the moment, the solution for this is to change your Host to a WebHost. If you don't do this at the moment you can't use the process described below for doing integration tests on Hosted Services.

### Issues with not knowing when the Act phase of the test is done

The problem is that you only want to do your asserts when the Hosted Service has finished it's work for your given test scenario. With this in mind, your basic test layout would be:

* Configure any mocks required and inject them in the test server
* Start the test server
* Wait for the Hosted Service to complete it's work for the given test case
* Stop the test server
* Do the asserts

Out of the box there is no way for you to know when the Hosted Service has finished the work. The simplistic solution is to always wait for a given period of time and then do the asserts. This is not the best solution cause it will depend on the hardware on which the tests are run and usually leads to flaky tests.

The provided solution will let you do this on a custom condition or as well as on time based condition if that's what you actually require.

## How to use

Start by creating an integration test as shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

Once you have your test ready configure the responses of the HttpClient by using the `WebApplicationFactory.RunUntilAsync` extension method.

Assume that we have an `ICalculator` type added to the `IServiceColletion` as part of the Startup of our WebHost. Now imagine that our Hosted Service makes use of the `ICalculator`, perhaps the Hosted Service was on an infinite loop doing some calculations.

Given this example you could implement a test that would wait for the `ICalculator.Sum` method to be called 3 times and only then do your asserts. See the DemoTest below:

```
public interface ICalculator
{
	int Sum(int left, int right);
}

public class HostedServiceDemoTests : IClassFixture<WebApplicationFactory<Startup>>
{
	private readonly WebApplicationFactory<Startup> _webApplicationFactory;

	public HostedServiceDemoTests(WebApplicationFactory<Startup> webApplicationFactory)
	{
		_webApplicationFactory = webApplicationFactory;
	}

	[Fact]
	public async Task DemoTest()
	{
		var callCount = 0;
		var someMock = Substitute.For<ICalculator>();
		someMock
			.Sum(Arg.Any<int>(), Arg.Any<int>())
			.Returns(1)
			.AndDoes(info => callCount++);

		await _webApplicationFactory
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddSingleton<ICalculator>(someMock);
				});
			})
			.RunUntilAsync(() => callCount == 3);

		// do some asserts
	}
}
```

The DemoTest is using the [NSubstitute library](https://github.com/nsubstitute/NSubstitute) for mocking the `ICalculator` but you could use whatever mocking library you prefer.

The main difference from the integration test examples shown in [introduction to integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests) is that you do not use the `WebApplicationFactory.CreateClient()` and then use the returned HttpClient do to calls into the test server but instead you use the `WebApplicationFactory.RunUntilAsync` extension method with a custom conditions that will control the lifetime of the test server when using Hosted Services.

**Note**: when thinking about your test scenario understand that your code running on your Hosted Service won't immediatly stop when the test condition is reached. In reality, the set condition is checked periodically to understand if the test server should be stopped.

To put it another way, the set condition actually means *don't stop the server before at least this condition is met*.

This is important when planning your stop condition and asserts as it might mean that more of your code executed than you might initially think if you don't plan your stop condition appropriately.

 As an example if your Hosted Service is in a while loop doing some operation and your keeping count of how many times that operation has run before stopping the test server, then the stop condition should probably be `numberOfRuns >= <some value>` instead of `numberOfRuns == <some value>`.

### Use a time condition to stop the test server

If you prefer to run the web server for a period of time before terminating it you can use the `WebApplicationFactory.RunUntilTimeoutAsync` extension method:

Given that you have an instance of `WebApplicationFactory` you can do someting like:

```
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

Usually it's best to consider stopping after a condition is met. Abusing the `WebApplicationFactory.RunUntilTimeoutAsync` and using it in scenarios where you could have set a condition using the `WebApplicationFactory.RunUntilAsync` might lead to flaky tests.

Furthermore the `WebApplicationFactory.RunUntilTimeoutAsync` is usually only useful if your Hosted Service implementation runs in some kind of loop condition that doesn't stop unless the server is shutdown.

### Configure a timeout for the condition set to stop the test server

When setting a condition to for the `WebApplicationFactory.RunUntilAsync` extension method there is a default timeout of 5 seconds set to reach that condition. If the condition is not reached the test server is stopped and a `RunUntilException` is thrown.

This is to avoid having a test that never ends because the set condition is never reached. You can configure this timeout by using the overloads `WebApplicationFactory.RunUntilAsync` extension method. Given the example [DemoTest shown above](#how-to-use) above you could configure a timeout by doing as follows:

```
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

Notice that when debugging the default timeout will not be 5 seconds, it will instead be 1 day. This is done so that you can take your time when debugging tests and not have the timeout being triggered and abort the test server in the middle of debugging.

The above is only true for the default timeout. Meaning that any timeout that you set is honored even when debugging.

Beware of this when you're debugging tests where you've set a low timeout. You might have to increase your set timeout to something large enough to let you debug your test properly and then once you're happy set it back to the desired timeout.

### Configuring the interval of time on which the condition is checked

When you set a condition, that condition is checked in a loop until it's reached or until the timeout is triggered:

* if it evaluates to true the server is stopped
* if it evaluates to false the condition is only checked after some time

By default the condition is checked in intervals of 5 milliseconds. This can be configured by using the overloads `WebApplicationFactory.RunUntilAsync` extension method. Given the example [DemoTest shown above](#how-to-use) above you could configure it as follows:

```
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

**Note**: when debugging it might be useful to set this to a larger period to allow you to step through your code more easily before the check for the condition kicks in and potentially shuts down the test server.

### Manually terminating the test server

You can use the overloads of the `WebApplicationFactory.RunUntilAsync` extension method to pass a `CancellationToken` which will stop the web server if cancelled.

## How to run the demo

The demo for this extension is represented by a test class.

* Go to the project `/demos/AspNetCore.Extensions.Testing.Demos/AspNetCore.Extensions.Testing.Demos.csproj`
* Run the tests for the class `HostedServicesDemoTests`