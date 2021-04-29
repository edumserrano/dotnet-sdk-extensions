using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience
{
    public class ResiliencePolicyHttpClientBuilderExtensionsTests
    {
        [Fact]
        public void AddResiliencePoliciesAddsPolicies()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var services = new ServiceCollection();
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");

            // fallback policy
            policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            policyHttpMessageHandlers[3]
                .GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void ExampleReflectionTest()
        {
            var policyKey = "testPolicy";
            var optionsName = "circuitBreakerOptions";
            var services = new ServiceCollection();

            AsyncRetryPolicy<HttpResponseMessage>? asyncRetryPolicy = null;
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    asyncRetryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            asyncRetryPolicy.ShouldNotBeNull();

            asyncRetryPolicy.GetRetryCount().ShouldBe(3);
            asyncRetryPolicy.GetMedianFirstRetryDelay().ShouldBe(TimeSpan.FromSeconds(1));

            var exceptionPredicates = asyncRetryPolicy.GetExceptionPredicates();
            exceptionPredicates.GetExceptionPredicatesCount().ShouldBe(3);
            exceptionPredicates.HandlesException<TaskCanceledException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<TimeoutRejectedException>().ShouldBeTrue();
            exceptionPredicates.HandlesException<HttpRequestException>().ShouldBeTrue();

            var resultPredicates = asyncRetryPolicy.GetResultPredicates();
            resultPredicates.GetResultPredicatesCount().ShouldBe(1);
            resultPredicates.HandlesTransientHttpStatusCode().ShouldBe(true);
        }

    }
}
