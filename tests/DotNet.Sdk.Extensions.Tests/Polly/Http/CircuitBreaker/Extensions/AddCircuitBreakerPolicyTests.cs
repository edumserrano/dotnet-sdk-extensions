using System;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Extensions
{
    /// <summary>
    /// Tests for the <see cref="CircuitBreakerHttpClientBuilderExtensions"/> class.
    /// Specifically for the CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy overloads.
    ///
    /// Many tests here use reflection to check that the policy is configured as expected.
    /// Although I'd prefer to do it without using reflection I couldn't find an alternative.
    /// At least not one that wouldn't force me to trigger the policy in different scenarios
    /// to check what I need. If I did that then it would almost be like testing that the Polly
    /// policies do what they are supposed to do and my intention is NOT to test the Polly code.
    ///
    /// Because of the reflection usage these tests can break when updating the Polly packages.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddCircuitBreakerPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="CircuitBreakerOptions"/>.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicy1()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldBeConfiguredAsExpected(
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput);
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyConfigurationType: typeof(DefaultCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts only the name of the option to use for the value of the <see cref="CircuitBreakerOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicy2()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldBeConfiguredAsExpected(
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput);
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyConfigurationType: typeof(DefaultCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts the name of the option to use for the value of the <see cref="CircuitBreakerOptions"/>
        /// and a <see cref="ICircuitBreakerPolicyEventHandler"/> type to handle the circuit break events.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicy3()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldBeConfiguredAsExpected(
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput);
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyConfigurationType: typeof(TestCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts the name of the option to use for the value of the <see cref="CircuitBreakerOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="CircuitBreakerOptionsExtensions.AddHttpClientCircuitBreakerOptions"/> extension method.
        ///
        /// This overload also accepts an <see cref="ICircuitBreakerPolicyEventHandler"/> type to handle the circuit break events.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicy4()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            
            circuitBreakerPolicy.ShouldNotBeNull();
            circuitBreakerPolicy.ShouldBeConfiguredAsExpected(
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput);
            circuitBreakerPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                durationOfBreakInSecs: durationOfBreakInSecs,
                samplingDurationInSecs: samplingDurationInSecs,
                failureThreshold: failureThreshold,
                minimumThroughput: minimumThroughput,
                policyConfigurationType: typeof(TestCircuitBreakerPolicyEventHandler));
        }
        
        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicyUniquePolicyPerHttpClient()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy1 = null;
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy2 = null;
            var durationOfBreakInSecs = 30;
            var samplingDurationInSecs = 60;
            var failureThreshold = 0.6;
            var minimumThroughput = 10;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = durationOfBreakInSecs;
                    options.SamplingDurationInSecs = samplingDurationInSecs;
                    options.FailureThreshold = failureThreshold;
                    options.MinimumThroughput = minimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            circuitBreakerPolicy1.ShouldNotBeNull();
            circuitBreakerPolicy2.ShouldNotBeNull();
            ReferenceEquals(circuitBreakerPolicy1, circuitBreakerPolicy2).ShouldBeFalse();
            circuitBreakerPolicy1.PolicyKey.ShouldNotBe(circuitBreakerPolicy2.PolicyKey);
        }
    }
}
