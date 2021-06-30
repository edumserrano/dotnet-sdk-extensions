using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to test that the fallback policy is added.
    ///
    /// Disables other policies to avoid triggering them when testing the fallback policy. 
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesFallbackTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy1()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Fallback
                .HttpClientShouldContainFallbackPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy2()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Fallback
                .HttpClientShouldContainFallbackPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="IFallbackPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Fallback.HttpClientShouldContainFallbackPolicyAsync();
            resiliencePoliciesAsserter.Fallback.EventHandlerShouldReceiveExpectedEvents(
                onHttpRequestExceptionCount: 1,
                onTimeoutCallsCount: 1,
                onBrokenCircuitCallsCount: 1,
                onIsolatedCircuitCallsCount: 1,
                onTaskCancelledCallsCount: 2,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Fallback);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Fallback.HttpClientShouldContainFallbackPolicyAsync();
            resiliencePoliciesAsserter.Fallback.EventHandlerShouldReceiveExpectedEvents(
                onHttpRequestExceptionCount: 1,
                onTimeoutCallsCount: 1,
                onBrokenCircuitCallsCount: 1,
                onIsolatedCircuitCallsCount: 1,
                onTaskCancelledCallsCount: 2,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Fallback);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string,Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy5()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName, provider =>
                {
                    return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Fallback.HttpClientShouldContainFallbackPolicyAsync();
            resiliencePoliciesAsserter.Fallback.EventHandlerShouldReceiveExpectedEvents(
                onHttpRequestExceptionCount: 1,
                onTimeoutCallsCount: 1,
                onBrokenCircuitCallsCount: 1,
                onIsolatedCircuitCallsCount: 1,
                onTaskCancelledCallsCount: 2,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Fallback);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions},Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a fallback policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy6()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(
                    configureOptions: options =>
                    {
                        options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                        options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                        options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    },
                    eventHandlerFactory: provider =>
                    {
                        return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                    })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Fallback.HttpClientShouldContainFallbackPolicyAsync();
            resiliencePoliciesAsserter.Fallback.EventHandlerShouldReceiveExpectedEvents(
                onHttpRequestExceptionCount: 1,
                onTimeoutCallsCount: 1,
                onBrokenCircuitCallsCount: 1,
                onIsolatedCircuitCallsCount: 1,
                onTaskCancelledCallsCount: 2,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Fallback);
        }

        /// <summary>
        /// Tests that the AddResiliencePolicies method does not add a fallback policy if
        /// the <see cref="ResilienceOptions.EnableFallbackPolicy"/> option is false.
        ///
        /// This test simulates a TimeoutRejectedException thrown by the timeout policy and
        /// because EnableFallbackPolicy is false the exception is not handled by the
        /// fallback policy.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy7()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableRetryPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.05
                },
            };
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var triggerTimeoutPath = "/timeout";
            var timeout = TimeSpan.FromSeconds(resilienceOptions.Timeout.TimeoutInSecs + 1);
            testHttpMessageHandler.HandleTimeout(triggerTimeoutPath, timeout);

            await Should.ThrowAsync<TimeoutRejectedException>(() => httpClient.GetAsync(triggerTimeoutPath));
        }
    }
}
