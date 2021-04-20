using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyRegistryExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class RetryPolicyRegistryExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="RetryPolicyRegistryExtensions.AddHttpClientRetryPolicy(IPolicyRegistry{string},string,string,IServiceProvider)"/>
        /// overload method adds the retry policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientRetryPolicy()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 3;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncRetryPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyRegistryExtensions.AddHttpClientRetryPolicy{TPolicyConfiguration}(IPolicyRegistry{string},string,string,IServiceProvider)"/>
        /// overload method adds the retry policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientRetryPolicy2()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 3;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy<TestRetryPolicyConfiguration>(policyKey, optionsName, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncRetryPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyRegistryExtensions.AddHttpClientRetryPolicy(IPolicyRegistry{string},string,string,IRetryPolicyConfiguration,IServiceProvider)"/>
        /// overload method adds the retry policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientRetryPolicy3()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 3;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncRetryPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyRegistryExtensions.AddHttpClientRetryPolicy(IPolicyRegistry{string},string,RetryOptions,IRetryPolicyConfiguration)"/>
        /// overload method adds the retry policy to the Polly registry
        /// </summary>
        [Fact]
        public void AddHttpClientRetryPolicy4()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var policyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
                var options = new RetryOptions
                {
                    RetryCount = 3,
                    MedianFirstRetryDelayInSecs = 1
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, options, policyConfiguration);
            });

            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry
                .TryGet<AsyncRetryPolicy<HttpResponseMessage>>(policyKey, out var policy)
                .ShouldBeTrue();
        }
        
        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method will not trigger the retry
        /// policy if not required and returns the expected value.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyControlTest()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.1
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            var callCount = 0;
            var httpResponseMessage = await retryPolicy.ExecuteAsync(
                () =>
                {
                    callCount++;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
                });
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Accepted);
            callCount.ShouldBe(1);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method configures the retry policy
        /// with the RetryOptions.RetryCount.
        ///
        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
        /// to test that the policy's configuration is as expected.
        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
        /// seem to be possible.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyHonorsOptions()
        {
            var policyKey = "testPolicy";
            var expectedRetryCount = 2;
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = expectedRetryCount,
                    MedianFirstRetryDelayInSecs = 0.1
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            var callCount = 0;
            await retryPolicy.ExecuteAndCaptureAsync(
                () =>
                {
                    callCount++;
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult(httpResponseMessage);
                });

            callCount.ShouldBe(expectedRetryCount + 1);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method will trigger the policy configuration
        /// when it retries.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersPolicyConfigurationOnRetry()
        {
            var policyKey = "testPolicy";
            var expectedRetryCount = 2;
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = expectedRetryCount,
                    MedianFirstRetryDelayInSecs = 0.1
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            await retryPolicy.ExecuteAndCaptureAsync(
                () =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult(httpResponseMessage);
                });

            await retryPolicyConfiguration
                .ReceivedWithAnyArgs(expectedRetryCount)
                .OnRetryAsync(
                    retryOptions: default!,
                    outcome: default!,
                    retryDelay: default,
                    retryNumber: default,
                    pollyContext: default!);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method will trigger the policy configuration
        /// when it retries and that the call to IRetryPolicyConfiguration.OnRetryAsync is done with the
        /// expected values.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyHonorsConfiguration()
        {
            RetryOptions retryOptionsFromPolicyConfiguration = null!;
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            retryPolicyConfiguration
                .WhenForAnyArgs(x =>
                    x.OnRetryAsync(
                        retryOptions: default!,
                        outcome: default!,
                        retryDelay: default,
                        retryNumber: default,
                        pollyContext: default!))
                .Do(callInfo =>
                {
                    retryOptionsFromPolicyConfiguration = callInfo.ArgAt<RetryOptions>(0);
                });
            var policyKey = "testPolicy";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = retryCount,
                    MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });
            
            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult(httpResponseMessage);
                });

            // Only asserting these two properties. Should I try to assert more?
            retryOptionsFromPolicyConfiguration.RetryCount.ShouldBe(retryCount);
            retryOptionsFromPolicyConfiguration.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method configures the retry policy
        /// to handle transient http errors.
        ///
        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
        /// to test that the policy's configuration is as expected.
        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
        /// seem to be possible.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersOnTransientHttpErrors()
        {
            var policyKey = "testPolicy";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = retryCount,
                    MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult(httpResponseMessage);
                });

            await retryPolicyConfiguration
                .ReceivedWithAnyArgs(retryCount)
                .OnRetryAsync(
                    retryOptions: default!,
                    outcome: default!,
                    retryDelay: default,
                    retryNumber: default,
                    pollyContext: default!);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method configures the retry policy
        /// to handle TimeoutRejectedException exceptions.
        ///
        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
        /// to test that the policy's configuration is as expected.
        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
        /// seem to be possible.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersOnTimeoutRejectedException()
        {
            var policyKey = "testPolicy";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = retryCount,
                    MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw new TimeoutRejectedException("test message");
                });

            await retryPolicyConfiguration
                .ReceivedWithAnyArgs(retryCount)
                .OnRetryAsync(
                    retryOptions: default!,
                    outcome: default!,
                    retryDelay: default,
                    retryNumber: default,
                    pollyContext: default!);
        }

        /// <summary>
        /// Tests that the IPolicyRegistry.AddHttpClientRetryPolicy method configures the retry policy
        /// to handle TaskCanceledException exceptions.
        ///
        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
        /// to test that the policy's configuration is as expected.
        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
        /// seem to be possible.
        /// </summary>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersOnTaskCancelledException()
        {
            var policyKey = "testPolicy";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                var retryOptions = new RetryOptions
                {
                    RetryCount = retryCount,
                    MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs
                };
                policyRegistry.AddHttpClientRetryPolicy(policyKey, retryOptions, retryPolicyConfiguration);
            });

            var retryPolicy = services.GetHttpPolicy<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw new TaskCanceledException("test message");
                });

            await retryPolicyConfiguration
                .ReceivedWithAnyArgs(retryCount)
                .OnRetryAsync(
                    retryOptions: default!,
                    outcome: default!,
                    retryDelay: default,
                    retryNumber: default,
                    pollyContext: default!);
        }
    }
}
