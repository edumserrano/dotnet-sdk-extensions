using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
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
            _ = services
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

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
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
            _ = services
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
            _ = services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
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
            _ = services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            _ = services
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

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
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
            _ = services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            _ = services
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
            _ = services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
                .GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string,Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder5()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            _ = services
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
            _ = services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(optionsName, provider =>
                {
                    return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
                .GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions},Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds all the policy handlers in the expected order.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesAddsPoliciesInOrder6()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var policyHttpMessageHandlers = new List<PolicyHttpMessageHandler>();
            var services = new ServiceCollection();
            _ = services
                .AddHttpClient("GitHub")
                .AddResiliencePolicies(
                    configureOptions: options =>
                    {
                        options.Timeout.TimeoutInSecs = 1;
                        options.Retry.MedianFirstRetryDelayInSecs = 1;
                        options.Retry.RetryCount = 3;
                        options.CircuitBreaker.DurationOfBreakInSecs = 1;
                        options.CircuitBreaker.FailureThreshold = 0.5;
                        options.CircuitBreaker.SamplingDurationInSecs = 60;
                        options.CircuitBreaker.MinimumThroughput = 4;
                    },
                    eventHandlerFactory: provider =>
                    {
                        return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                    })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    policyHttpMessageHandlers = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");

            policyHttpMessageHandlers.Count.ShouldBe(4);
            // fallback policy
            _ = policyHttpMessageHandlers[0]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // retry policy
            _ = policyHttpMessageHandlers[1]
                .GetPolicy<AsyncRetryPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // circuit breaker policy 
            _ = policyHttpMessageHandlers[2]
                .GetPolicy<AsyncPolicyWrap<HttpResponseMessage>>()
                .ShouldNotBeNull();
            // timeout policy
            _ = policyHttpMessageHandlers[3]
                .GetPolicy<AsyncTimeoutPolicy<HttpResponseMessage>>()
                .ShouldNotBeNull();
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// ResiliencePolicyHttpClientBuilderExtensions.AddResiliencePolicies methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddResiliencePoliciesUniquePolicyPerHttpClient()
        {
            var policyHttpMessageHandlers1 = new List<PolicyHttpMessageHandler>();
            var policyHttpMessageHandlers2 = new List<PolicyHttpMessageHandler>();
            var services = new ServiceCollection();
            _ = services
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
                    policyHttpMessageHandlers1 = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });
            _ = services
                .AddHttpClient("Microsoft")
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
                    policyHttpMessageHandlers2 = httpMessageHandlerBuilder
                        .AdditionalHandlers
                        .OfType<PolicyHttpMessageHandler>()
                        .ToList();
                });

            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.InstantiateNamedHttpClient("GitHub");
            _ = serviceProvider.InstantiateNamedHttpClient("Microsoft");

            var resiliencePolicies1 = new ResiliencePolicies(policyHttpMessageHandlers1);
            _ = resiliencePolicies1.TimeoutPolicy.ShouldNotBeNull();
            _ = resiliencePolicies1.RetryPolicy.ShouldNotBeNull();
            _ = resiliencePolicies1.CircuitBreakerPolicy.ShouldNotBeNull();
            _ = resiliencePolicies1.FallbackPolicy.ShouldNotBeNull();

            var resiliencePolicies2 = new ResiliencePolicies(policyHttpMessageHandlers2);
            _ = resiliencePolicies2.TimeoutPolicy.ShouldNotBeNull();
            _ = resiliencePolicies2.RetryPolicy.ShouldNotBeNull();
            _ = resiliencePolicies2.CircuitBreakerPolicy.ShouldNotBeNull();
            _ = resiliencePolicies2.FallbackPolicy.ShouldNotBeNull();

            ReferenceEquals(resiliencePolicies1.TimeoutPolicy, resiliencePolicies2.TimeoutPolicy).ShouldBeFalse();
            resiliencePolicies1.TimeoutPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.TimeoutPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.RetryPolicy, resiliencePolicies2.RetryPolicy).ShouldBeFalse();
            resiliencePolicies1.RetryPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.RetryPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.CircuitBreakerPolicy, resiliencePolicies2.CircuitBreakerPolicy).ShouldBeFalse();
            resiliencePolicies1.CircuitBreakerPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.CircuitBreakerPolicy.PolicyKey);
            ReferenceEquals(resiliencePolicies1.FallbackPolicy, resiliencePolicies2.FallbackPolicy).ShouldBeFalse();
            resiliencePolicies1.FallbackPolicy.PolicyKey.ShouldNotBe(resiliencePolicies2.FallbackPolicy.PolicyKey);
        }
    }
}
