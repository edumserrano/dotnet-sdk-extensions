using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder1()
        {
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var services = new ServiceCollection();
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

            policyHttpMessageHandlers.Count.ShouldBe(4);
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

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder2()
        {
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                });
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
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

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
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

            policyHttpMessageHandlers.Count.ShouldBe(4);
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

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;
                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;
                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                });
            services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
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
    }
}
