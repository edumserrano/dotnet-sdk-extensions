//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using DotNet.Sdk.Extensions.Polly;
//using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
//using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
//using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
//using DotNet.Sdk.Extensions.Polly.Policies;
//using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
//using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
//using Microsoft.Extensions.DependencyInjection;
//using NSubstitute;
//using Polly.CircuitBreaker;
//using Polly.Registry;
//using Polly.Timeout;
//using Polly.Wrap;
//using Shouldly;
//using Xunit;

//namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker
//{
//    /// <summary>
//    /// Tests for the <see cref="CircuitBreakerPolicyRegistryExtensions"/> class
//    /// </summary>
//    [Trait("Category", XUnitCategories.Polly)]
//    public class CircuitBreakerPolicyRegistryExtensionsTests
//    {
//        /// <summary>
//        /// Tests that the <see cref="CircuitBreakerPolicyRegistryExtensions.AddHttpClientCircuitBreakerPolicy(IPolicyRegistry{string},string,string,IServiceProvider)"/>
//        /// overload method adds the circuit breaker policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientCircuitBreakerPolicy()
//        {
//            var policyKey = "testPolicy";
//            var optionsName = "circuitBreakerOptions";
//            var services = new ServiceCollection();
//            services
//                .AddHttpClientCircuitBreakerOptions(optionsName)
//                .Configure(options =>
//                {
//                    options.DurationOfBreakInSecs = 1;
//                    options.FailureThreshold = 0.8;
//                    options.MinimumThroughput = 2;
//                    options.SamplingDurationInSecs = 60;
//                });
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, optionsName, provider);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the <see cref="CircuitBreakerPolicyRegistryExtensions.AddHttpClientCircuitBreakerPolicy{TPolicyConfiguration}(IPolicyRegistry{string},string,string,IServiceProvider)"/>
//        /// overload method adds the circuit breaker policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientCircuitBreakerPolicy2()
//        {
//            var policyKey = "testPolicy";
//            var optionsName = "circuitBreakerOptions";
//            var services = new ServiceCollection();
//            services
//                .AddHttpClientCircuitBreakerOptions(optionsName)
//                .Configure(options =>
//                {
//                    options.DurationOfBreakInSecs = 1;
//                    options.FailureThreshold = 0.8;
//                    options.MinimumThroughput = 2;
//                    options.SamplingDurationInSecs = 60;
//                });
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientCircuitBreakerPolicy<TestCircuitBreakerPolicyConfiguration>(policyKey, optionsName, provider);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the <see cref="CircuitBreakerPolicyRegistryExtensions.AddHttpClientCircuitBreakerPolicy(IPolicyRegistry{string},string,string,ICircuitBreakerPolicyConfiguration,IServiceProvider)"/>
//        /// overload method adds the circuit breaker policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientCircuitBreakerPolicy3()
//        {
//            var policyKey = "testPolicy";
//            var optionsName = "circuitBreakerOptions";
//            var services = new ServiceCollection();
//            services
//                .AddHttpClientCircuitBreakerOptions(optionsName)
//                .Configure(options =>
//                {
//                    options.DurationOfBreakInSecs = 1;
//                    options.FailureThreshold = 0.8;
//                    options.MinimumThroughput = 2;
//                    options.SamplingDurationInSecs = 60;
//                });
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, optionsName, circuitBreakerPolicyConfiguration, provider);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the <see cref="CircuitBreakerPolicyRegistryExtensions.AddHttpClientCircuitBreakerPolicy(IPolicyRegistry{string},string,CircuitBreakerOptions,ICircuitBreakerPolicyConfiguration)"/>
//        /// overload method adds the circuit breaker policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientCircuitBreakerPolicy4()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var policyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.8,
//                    MinimumThroughput = 2,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, policyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method will not trigger the circuit
//        /// breaker policy if not required and returns the expected value.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyControlTest()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                var httpResponseMessage = await wrappedCircuitBreakerPolicy.ExecuteAsync(
//                    () =>
//                    {
//                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
//                    });
//                httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Accepted);
//            }

//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(0)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy with the CircuitBreakerOptions.MinimumThroughput.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyHonorsMinimumThroughputOption()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput + 1; i++)
//            {
//                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//                if (i >= minimumThroughput)
//                {
//                    policyResult.FinalException.ShouldBeNull();
//                    policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
//                }
//            }

//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy with the CircuitBreakerOptions.FailureThreshold.
//        /// 
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyHonorsFailureThresholdOption()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.6,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            // trigger the circuit breaker by exceeding the failure threshold
//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//            }

//            // reset the circuit breaker and do some failures but not enough to trigger the circuit breaker by
//            // staying under the failure threshold
//            var circuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;
//            circuitBreakerPolicy.Reset();
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    action: () =>
//                    {
//                        // since failureThreshold is 60%, if we fail only 50% it won't trigger 
//                        var httpResponseMessage = i % 2 == 0
//                            ? new HttpResponseMessage(HttpStatusCode.OK)
//                            : new HttpResponseMessage(HttpStatusCode.InternalServerError);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//            }

//            // show that the circuit breaker was only triggered once
//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }


//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy with the CircuitBreakerOptions.FailureThreshold.
//        /// Also tests that the <see cref="ICircuitBreakerPolicyConfiguration"/> is invoked when required.
//        /// 
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyHonorsDurationOfBreakOption()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var durationOfBreakInSecs = 1;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = durationOfBreakInSecs,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            // trigger the circuit breaker
//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//            }

//            // the ICircuitBreakerPolicyConfiguration.OnBreakAsync was triggered but not the
//            // ICircuitBreakerPolicyConfiguration.OnResetAsync
//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(0)
//                .OnResetAsync(
//                    circuitBreakerOptions: default!,
//                    context: default!);

//            // wait for the durationOfBreakInSecs which should reset the circuit breaker
//            // and trigger the ICircuitBreakerPolicyConfiguration.OnResetAsync
//            // and put some successful traffic through which should then trigger the
//            // ICircuitBreakerPolicyConfiguration.OnHalfOpenAsync
//            await Task.Delay(TimeSpan.FromSeconds(durationOfBreakInSecs + 1));
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    action: () =>
//                    {
//                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//            }

//            // check that the ICircuitBreakerPolicyConfiguration.OnResetAsync
//            // and ICircuitBreakerPolicyConfiguration.OnHalfOpenAsync is triggered
//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnResetAsync(
//                    circuitBreakerOptions: default!,
//                    context: default!);
//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnHalfOpenAsync(circuitBreakerOptions: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy to handle transient http errors.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTransientHttpErrors()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
//                        return Task.FromResult(httpResponseMessage);
//                    });
//            }

//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy to handle <see cref="TimeoutRejectedException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTimeoutRejectedException()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        throw new TimeoutRejectedException("test");
//                    });
//            }

//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method configures the
//        /// circuit breaker policy to handle <see cref="TaskCanceledException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyTriggersOnTaskCancelledException()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            for (var i = 0; i < minimumThroughput; i++)
//            {
//                await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        throw new TaskCanceledException("test");
//                    });
//            }

//            await circuitBreakerPolicyConfiguration
//                .ReceivedWithAnyArgs(1)
//                .OnBreakAsync(
//                    circuitBreakerOptions: default!,
//                    lastOutcome: default!,
//                    previousState: default,
//                    durationOfBreak: default,
//                    context: default!);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method wraps the circuit breaker
//        /// policy with the <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/> which avoids
//        /// <see cref="BrokenCircuitException"/> exceptions from being thrown when the circuit is open.
//        /// Instead it returns a <see cref="CircuitBrokenHttpResponseMessage"/>.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyContainsCircuitBreakerCheckerPolicy()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var exception = new TaskCanceledException("test");
//            for (var i = 0; i < minimumThroughput + 1; i++)
//            {
//                var policyResult = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        throw exception;
//                    });
//                if (i < minimumThroughput)
//                {
//                    // when the circuit is not open
//                    policyResult.FinalException.ShouldBe(exception);
//                    policyResult.Result.ShouldBeNull();
//                }
//                else
//                {
//                    // when the circuit is open
//                    policyResult.FinalException.ShouldBeNull();
//                    policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
//                }
//            }
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientCircuitBreakerPolicy method wraps the circuit breaker
//        /// policy with the <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/> which avoids
//        /// <see cref="IsolatedCircuitException"/> exceptions from being thrown when the circuit is isolated.
//        /// Instead it returns a <see cref="CircuitBrokenHttpResponseMessage"/>.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientCircuitBreakerPolicyContainsCircuitBreakerCheckerPolicy2()
//        {
//            var policyKey = "testPolicy";
//            var minimumThroughput = 4;
//            var services = new ServiceCollection();
//            var circuitBreakerPolicyConfiguration = Substitute.For<ICircuitBreakerPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var options = new CircuitBreakerOptions
//                {
//                    DurationOfBreakInSecs = 1,
//                    FailureThreshold = 0.5,
//                    MinimumThroughput = minimumThroughput,
//                    SamplingDurationInSecs = 60
//                };
//                policyRegistry.AddHttpClientCircuitBreakerPolicy(policyKey, options, circuitBreakerPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var wrappedCircuitBreakerPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var circuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;
//            var exception = new TaskCanceledException("test");

//            var policyResult1 = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                    () =>
//                    {
//                        throw exception;
//                    });
//            // when the circuit is closed (not isolated)
//            policyResult1.FinalException.ShouldBe(exception);
//            policyResult1.Result.ShouldBeNull();

//            circuitBreakerPolicy.Isolate();
//            var policyResult2 = await wrappedCircuitBreakerPolicy.ExecuteAndCaptureAsync(
//                () =>
//                {
//                    throw exception;
//                });
//            // when the circuit is isolated
//            policyResult2.FinalException.ShouldBeNull();
//            policyResult2.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
//        }
//    }
//}
