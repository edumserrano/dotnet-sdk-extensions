using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.FallbackHttpResponseMessages;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.HttpClient.Fallback
{
    [Trait("Category", "Polly")]
    public class HttpClientFallbackPolicyTests
    {
        [Fact]
        public async Task AddHttpClientFallbackPolicyTriggersOnTimeoutRejectedException()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            var exception = new TimeoutRejectedException("test message");
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw exception;
                });
            policyResult.FinalException.ShouldBeNull();
            policyResult.Result.ShouldBeOfType<TimeoutHttpResponseMessage>();
            var result = (TimeoutHttpResponseMessage)policyResult.Result;
            result.Exception.ShouldBe(exception);
        }

        [Fact]
        public async Task AddHttpClientFallbackPolicyTriggersOnBrokenCircuitException()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            var exception = new BrokenCircuitException();
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw exception;
                });
            policyResult.FinalException.ShouldBeNull();
            policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
            var result = (CircuitBrokenHttpResponseMessage)policyResult.Result;
            result.Exception.ShouldBe(exception);
        }

        [Fact]
        public async Task AddHttpClientFallbackPolicyTriggersOnTaskCanceledException()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            var exception = new TaskCanceledException("test message");
            var policyResult = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw exception;
                });
            policyResult.FinalException.ShouldBeNull();
            policyResult.Result.ShouldBeOfType<AbortedHttpResponseMessage>();
            var result = (AbortedHttpResponseMessage)policyResult.Result;
            result.Exception.ShouldBe(exception);
        }

        [Fact]
        public async Task AddHttpClientFallbackPolicyTriggersOnTaskCanceledException2()
        {
            var policyKey = "testPolicy";
            var services = new ServiceCollection();
            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
            });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);

            var taskCancelledException1 = new TaskCanceledException("test message");
            var policyResult1 = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw taskCancelledException1;
                });
            var result1 = (AbortedHttpResponseMessage)policyResult1.Result;
            result1.TriggeredByTimeoutException.ShouldBeFalse();

            var innerException2 = new TimeoutException();
            var taskCancelledException2 = new TaskCanceledException("test message", innerException2);
            var policyResult2 = await retryPolicy.ExecuteAndCaptureAsync(
                action: () =>
                {
                    throw taskCancelledException2;
                });
            var result2 = (AbortedHttpResponseMessage)policyResult2.Result;
            result2.TriggeredByTimeoutException.ShouldBeTrue();
        }
    }
}
