using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.CircuitBreaker.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Timeout;
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

        /// <summary>
        /// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        /// don't know how to test the MedianFirstRetryDelayInSecs....
        /// also tests policy configuration
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyHonorsMinimumThroughputOption()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.8;
            var minimumThroughput = 2;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        return Task.FromResult(httpResponseMessage);
                    });
            }

            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
        }

        /// <summary>
        /// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        /// don't know how to test the MedianFirstRetryDelayInSecs....
        /// also tests policy configuration
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyHonorsFailureThresholdOption()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.6;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        return Task.FromResult(httpResponseMessage);
                    });
            }
            var circuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;
            circuitBreakerPolicy.Reset();
            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        // since failureThreshold is 60%, if we fail only 50% it won't trigger 
                        var httpResponseMessage = i % 2 == 0
                            ? new HttpResponseMessage(HttpStatusCode.OK)
                            : new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        return Task.FromResult(httpResponseMessage);
                    });
            }

            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
        }

        /// <summary>
        /// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        /// don't know how to test the MedianFirstRetryDelayInSecs....
        ///
        /// also tests policy configuration
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyHonorsDurationOfBreakOption()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        return Task.FromResult(httpResponseMessage);
                    });
            }
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(0)
                .OnResetAsync(
                    circuitBreakerOptions: default!,
                    context: default!);

            await Task.Delay(TimeSpan.FromSeconds(durationOfBreakInSecs + 1));
            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        return Task.FromResult(httpResponseMessage);
                    });
            }
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnResetAsync(
                    circuitBreakerOptions: default!,
                    context: default!);
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnHalfOpenAsync(circuitBreakerOptions: default!);
        }

        /// <summary>
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTimeoutRejectedException()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        throw new TimeoutRejectedException("test");
                    });
            }
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
        }

        /// <summary>
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTransientHttpErrors()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        return Task.FromResult(httpResponseMessage);
                    });
            }
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
        }

        /// <summary>
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTaskCancelledException()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            for (var i = 0; i < minimumThroughput; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        throw new TaskCanceledException("test");
                    });
            }
            await circuitBreakerPolicyConfiguration
                .ReceivedWithAnyArgs(1)
                .OnBreakAsync(
                    circuitBreakerOptions: default!,
                    lastOutcome: default!,
                    previousState: default,
                    durationOfBreak: default,
                    context: default!);
        }

        [Fact]
        public async Task AddHttpClientCircuitBreakerPolicyContainsCircuitBreakerCheckerPolicy()
        {
            var policyKey = "testPolicy";
            var durationOfBreakInSecs = 1;
            var failureThreshold = 0.5;
            var minimumThroughput = 4;
            var samplingDurationInSecs = 60;
            var services = new ServiceCollection();
            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var options = new CircuitBreakerOptions
                {
                    DurationOfBreakInSecs = durationOfBreakInSecs,
                    FailureThreshold = failureThreshold,
                    MinimumThroughput = minimumThroughput,
                    SamplingDurationInSecs = samplingDurationInSecs
                };
                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var wrappedCircuitBreakerPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            var exception = new TaskCanceledException("test");
            for (var i = 0; i < minimumThroughput + 1; i++)
            {
                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
                    action: () =>
                    {
                        throw exception;
                    });
                if (i < minimumThroughput)
                {
                    policyResult.FinalException.ShouldBe(exception);
                    policyResult.Result.ShouldBeNull();
                }
                else
                {
                    policyResult.FinalException.ShouldBeNull();
                    policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
                }
            }

        }
    }
}
