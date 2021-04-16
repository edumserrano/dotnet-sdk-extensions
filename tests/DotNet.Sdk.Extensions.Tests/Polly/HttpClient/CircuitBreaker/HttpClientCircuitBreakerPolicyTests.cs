using System.Net.Http;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.CircuitBreaker.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly.Registry;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.CircuitBreaker
{
    [Trait("Category", "Polly")]
    public class HttpClientCircuitBreakerPolicyTests
    {
        [Fact]
        public void AddHttpClientCircuitBreakerOptions()
        {
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                });
            var serviceProvider = services.BuildServiceProvider();
            var circuitBreakerOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<CircuitBreakerOptions>>();
            circuitBreakerOptionsMonitor.ShouldNotBeNull();
            var circuitBreakerOptions = circuitBreakerOptionsMonitor.Get(optionsName);
            circuitBreakerOptions.DurationOfBreakInSecs.ShouldBe(durationOfBreakInSecs);
            circuitBreakerOptions.FailureThreshold.ShouldBe(failureThreshold);
            circuitBreakerOptions.MinimumThroughput.ShouldBe(minimumThroughput);
            circuitBreakerOptions.SamplingDurationInSecs.ShouldBe(samplingDurationInSecs);
        }

        [Fact]
        public void AddHttpClientCircuitBreakerPolicyWithDefaultConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncPolicyWrap<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientCircuitBreakerPolicyWithConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientCircuitBreakerPolicy<TestCircuitBreakerPolicyConfiguration>(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncPolicyWrap<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientCircuitBreakerPolicyWithConfiguration2()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, optionsName, circuitBreakerPolicyConfiguration, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncPolicyWrap<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        [Fact]
        public void AddHttpClientCircuitBreakerPolicyWithConfiguration3()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, policyConfiguration);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncPolicyWrap<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        ///// <summary>
        ///// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        ///// don't know how to test the MedianFirstRetryDelayInSecs....
        ///// </summary>
        ///// <returns></returns>
        //[Fact]
        //public async Task AddHttpClientRetryPolicyHonorsOptions()
        //{
        //    var policyKey = "testPolicy";
        //    var optionsName = "retryOptions";
        //    var retryCount = 2;
        //    var services = new ServiceCollection();
        //    services
        //        .AddHttpClientRetryOptions(optionsName)
        //        .Configure(options =>
        //        {
        //            options.RetryCount = retryCount;
        //            options.MedianFirstRetryDelayInSecs = 0.1;
        //        });
        //    var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
        //    services.AddPolicyRegistry((provider, policyRegistry) =>
        //    {
        //        policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
        //    });
        //    var serviceProvider = services.BuildServiceProvider();
        //    var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
        //    var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

        //    var cts = new CancellationTokenSource();
        //    var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
        //        action: (context, cancellationToken) =>
        //        {
        //            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //            return Task.FromResult(httpResponseMessage);
        //        },
        //        context: new Context(),
        //        cancellationToken: cts.Token);

        //    await retryPolicyConfiguration
        //        .ReceivedWithAnyArgs(retryCount)
        //        .OnRetryAsync(
        //            retryOptions: default!,
        //            outcome: default!,
        //            retryDelay: default,
        //            retryNumber: default,
        //            pollyContext: default!);
        //}

        ///// <summary>
        ///// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        ///// note about only testing the retry options when the configuration is invoked... not sure how to test the rest
        ///// </summary>
        ///// <returns></returns>
        //[Fact]
        //public async Task AddHttpClientTimeoutPolicyHonorsConfiguration()
        //{
        //    var policyKey = "testPolicy";
        //    var optionsName = "retryOptions";
        //    var retryCount = 2;
        //    var medianFirstRetryDelayInSecs = 0.1;
        //    var services = new ServiceCollection();
        //    services
        //        .AddHttpClientRetryOptions(optionsName)
        //        .Configure(options =>
        //        {
        //            options.RetryCount = retryCount;
        //            options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
        //        });
        //    RetryOptions retryOptions = null!;
        //    var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
        //    retryPolicyConfiguration
        //        .WhenForAnyArgs(x=>
        //            x.OnRetryAsync(
        //                retryOptions: default!,
        //                outcome: default!,
        //                retryDelay: default,
        //                retryNumber: default,
        //                pollyContext: default!))
        //        .Do(callInfo =>
        //        {
        //            retryOptions = callInfo.ArgAt<RetryOptions>(0);
        //        });
        //    services.AddPolicyRegistry((provider, policyRegistry) =>
        //    {
        //        policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
        //    });
        //    var serviceProvider = services.BuildServiceProvider();
        //    var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
        //    var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

        //    var cts = new CancellationTokenSource();
        //    var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
        //        action: (context, cancellationToken) =>
        //        {
        //            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //            return Task.FromResult(httpResponseMessage);
        //        },
        //        context: new Context(),
        //        cancellationToken: cts.Token);

        //    retryOptions.RetryCount.ShouldBe(retryCount);
        //    retryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
        //}

        /// <summary>
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        //[Fact]
        //public async Task AddHttpClientRetryPolicyTriggersOnTimeoutRejectedException()
        //{
        //    var policyKey = "testPolicy";
        //    var optionsName = "retryOptions";
        //    var retryCount = 2;
        //    var medianFirstRetryDelayInSecs = 0.1;
        //    var services = new ServiceCollection();
        //    services
        //        .AddHttpClientRetryOptions(optionsName)
        //        .Configure(options =>
        //        {
        //            options.RetryCount = retryCount;
        //            options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
        //        });
        //    var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
        //    services.AddPolicyRegistry((provider, policyRegistry) =>
        //    {
        //        policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
        //    });
        //    var serviceProvider = services.BuildServiceProvider();
        //    var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
        //    var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

        //    var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
        //        action: () =>
        //        {
        //            throw new TimeoutRejectedException("test message");
        //        });

        //    await retryPolicyConfiguration
        //        .ReceivedWithAnyArgs(retryCount)
        //        .OnRetryAsync(
        //            retryOptions: default!,
        //            outcome: default!,
        //            retryDelay: default,
        //            retryNumber: default,
        //            pollyContext: default!);
        //}

        ///// <summary>
        ///// it's not like I want to test polly itself but I want to make sure the policy
        ///// triggers are correctly configured and I don't know another way 
        ///// </summary>
        ///// <returns></returns>
        //[Fact]
        //public async Task AddHttpClientRetryPolicyTriggersOnTransientHttpErrors()
        //{
        //    var policyKey = "testPolicy";
        //    var optionsName = "retryOptions";
        //    var retryCount = 2;
        //    var medianFirstRetryDelayInSecs = 0.1;
        //    var services = new ServiceCollection();
        //    services
        //        .AddHttpClientRetryOptions(optionsName)
        //        .Configure(options =>
        //        {
        //            options.RetryCount = retryCount;
        //            options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
        //        });
        //    var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
        //    services.AddPolicyRegistry((provider, policyRegistry) =>
        //    {
        //        policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
        //    });
        //    var serviceProvider = services.BuildServiceProvider();
        //    var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
        //    var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

        //    var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
        //        action: () =>
        //        {
        //            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //            return Task.FromResult(httpResponseMessage);
        //        });

        //    await retryPolicyConfiguration
        //        .ReceivedWithAnyArgs(retryCount)
        //        .OnRetryAsync(
        //            retryOptions: default!,
        //            outcome: default!,
        //            retryDelay: default,
        //            retryNumber: default,
        //            pollyContext: default!);
        //}
    }
}
