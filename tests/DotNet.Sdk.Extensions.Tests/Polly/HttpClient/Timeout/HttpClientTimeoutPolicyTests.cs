using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Timeout
{
    [Trait("Category", "Polly")]
    public class HttpClientTimeoutPolicyTests
    {
        [Fact]
        public void AddHttpClientTimeoutOptions()
        {
            var optionsName = "timeoutOptions";
            var timeoutInSecs = 3;
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            var serviceProvider = services.BuildServiceProvider();
            var timeoutOptions = serviceProvider.GetHttpClientTimeoutOptions(optionsName);
            timeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
        }

        [Fact]
        public void AddHttpClientTimeoutPolicyWithDefaultConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientTimeoutPolicyWithConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy<TestTimeoutPolicyConfiguration>(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientTimeoutPolicyWithConfiguration2()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = 1);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, policyConfiguration, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientTimeoutPolicyWithConfiguration3()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
                var options = new TimeoutOptions
                {
                    TimeoutInSecs = 1
                };
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, options, policyConfiguration);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public async Task AddHttpClientTimeoutPolicyHonorsOptions()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);

            var cts = new CancellationTokenSource();
            var policyResult = await timeoutPolicy.ExecuteAndCaptureAsync(
                action: async (context, cancellationToken) =>
                {
                    var timeoutSpan = TimeSpan.FromSeconds(timeoutInSecs + 1);
                    cts.CancelAfter(timeoutSpan);
                    await Task.Delay(timeoutSpan, cancellationToken);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                },
                context: new Context(),
                cancellationToken: cts.Token);

            policyResult.FinalException.ShouldBeOfType<TimeoutRejectedException>();
        }

        [Fact]
        public async Task AddHttpClientTimeoutPolicyHonorsConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            var timeoutPolicyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, timeoutPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);

            var cts = new CancellationTokenSource();
            await timeoutPolicy.ExecuteAndCaptureAsync(
                action: async (context, cancellationToken) =>
                {
                    var timeoutSpan = TimeSpan.FromSeconds(timeoutInSecs + 1);
                    cts.CancelAfter(timeoutSpan);
                    await Task.Delay(timeoutSpan, cancellationToken);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                },
                context: new Context(),
                cancellationToken: cts.Token);

            await timeoutPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnTimeoutASync(
                    timeoutOptions: default!,
                    context: default!,
                    requestTimeout: default,
                    timedOutTask: default!,
                    exception: default!);
        }
        [Fact]
        public async Task AddHttpClientTimeoutPolicyHonorsConfiguration2()
        {
            var policyKey = "testPolicy";
            var optionsName = "timeoutOptions";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            TimeoutOptions timeoutOptions = null!;
            var requestTimeout = TimeSpan.Zero;
            var timeoutPolicyConfiguration = Substitute.For<ITimeoutPolicyConfiguration>();
            timeoutPolicyConfiguration
                .WhenForAnyArgs(x =>
                    x.OnTimeoutASync(
                        timeoutOptions: default!,
                        context: default!,
                        requestTimeout: default,
                        timedOutTask: default!,
                        exception: default!))
                .Do(callInfo =>
                {
                    timeoutOptions = callInfo.ArgAt<TimeoutOptions>(0);
                    requestTimeout = callInfo.ArgAt<TimeSpan>(2);
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientTimeoutPolicy(policyKey, optionsName, timeoutPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<AsyncTimeoutPolicy<HttpResponseMessage>>(policyKey);

            var cts = new CancellationTokenSource();
            await timeoutPolicy.ExecuteAndCaptureAsync(
                action: async (context, cancellationToken) =>
                {
                    var timeoutSpan = TimeSpan.FromSeconds(timeoutInSecs + 1);
                    cts.CancelAfter(timeoutSpan);
                    await Task.Delay(timeoutSpan, cancellationToken);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                },
                context: new Context(),
                cancellationToken: cts.Token);

            timeoutOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
            requestTimeout.ShouldBe(TimeSpan.FromSeconds(timeoutInSecs));
        }
    }
}
