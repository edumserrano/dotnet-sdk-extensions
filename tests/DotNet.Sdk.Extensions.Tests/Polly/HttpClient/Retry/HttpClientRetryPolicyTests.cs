using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Retry
{
    [Trait("Category", "Polly")]
    public class HttpClientRetryPolicyTests
    {
        [Fact]
        public void AddHttpClientRetryOptions()
        {
            var optionsName = "retryOptions";
            var retryCount = 3;
            var medianFirstRetryDelayInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });
            var serviceProvider = services.BuildServiceProvider();
            var retryOptions = serviceProvider.GetHttpClientRetryOptions(optionsName);
            retryOptions.RetryCount.ShouldBe(retryCount);
            retryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
        }
        
        [Fact]
        public void AddHttpClientRetryPolicyWithDefaultConfiguration()
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

        [Fact]
        public void AddHttpClientRetryPolicyWithConfiguration()
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

        [Fact]
        public void AddHttpClientRetryPolicyWithConfiguration2()
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

        [Fact]
        public void AddHttpClientRetryPolicyWithConfiguration3()
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
        /// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        /// don't know how to test the MedianFirstRetryDelayInSecs....
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientRetryPolicyHonorsOptions()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var retryCount = 2;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = 0.1;
                });
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
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
        /// it's not like I want to test polly itself but don't have another way to check the policy configuration?
        /// note about only testing the retry options when the configuration is invoked... not sure how to test the rest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientRetryPolicyHonorsConfiguration()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });
            RetryOptions retryOptions = null!;
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            retryPolicyConfiguration
                .WhenForAnyArgs(x=>
                    x.OnRetryAsync(
                        retryOptions: default!,
                        outcome: default!,
                        retryDelay: default,
                        retryNumber: default,
                        pollyContext: default!))
                .Do(callInfo =>
                {
                    retryOptions = callInfo.ArgAt<RetryOptions>(0);
                });
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);
            
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    return Task.FromResult(httpResponseMessage);
                });

            retryOptions.RetryCount.ShouldBe(retryCount);
            retryOptions.MedianFirstRetryDelayInSecs.ShouldBe(medianFirstRetryDelayInSecs);
        }

        /// <summary>
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersOnTimeoutRejectedException()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
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
        /// it's not like I want to test polly itself but I want to make sure the policy
        /// triggers are correctly configured and I don't know another way 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddHttpClientRetryPolicyTriggersOnTransientHttpErrors()
        {
            var policyKey = "testPolicy";
            var optionsName = "retryOptions";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 0.1;
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                });
            var retryPolicyConfiguration = Substitute.For<IRetryPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientRetryPolicy(policyKey, optionsName, retryPolicyConfiguration, provider);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncRetryPolicy<HttpResponseMessage>>(policyKey);

            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
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
    }
}
