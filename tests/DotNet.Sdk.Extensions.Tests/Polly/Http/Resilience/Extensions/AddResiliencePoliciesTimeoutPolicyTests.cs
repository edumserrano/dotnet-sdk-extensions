using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to test that the timeout policy is added.
    ///
    /// Disables other policies to avoid triggering them when testing the timeout policy.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesTimeoutPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy1()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Timeout
                .HttpClientShouldContainTimeoutPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy2()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            const string optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Timeout
                .HttpClientShouldContainTimeoutPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
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
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout.HttpClientShouldContainTimeoutPolicyAsync();
            resiliencePoliciesAsserter.Timeout.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            const string optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout.HttpClientShouldContainTimeoutPolicyAsync();
            resiliencePoliciesAsserter.Timeout.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string,Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy5()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            const string optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName, _ =>
                {
                    return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout.HttpClientShouldContainTimeoutPolicyAsync();
            resiliencePoliciesAsserter.Timeout.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions},Func{IServiceProvider,IResiliencePoliciesEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy6()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(
                    configureOptions: options =>
                    {
                        options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                        options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                        options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                        options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                    },
                    eventHandlerFactory: _ =>
                    {
                        return new TestResiliencePoliciesEventHandler(resiliencePoliciesEventHandlerCalls);
                    })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Timeout.HttpClientShouldContainTimeoutPolicyAsync();
            resiliencePoliciesAsserter.Timeout.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Timeout);
        }

        /// <summary>
        /// Tests that the AddResiliencePolicies method does not add a timeout policy if
        /// the <see cref="ResilienceOptions.EnableTimeoutPolicy"/> option is false.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsTimeoutPolicy7()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            const string httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Timeout = new TimeoutOptions
                {
                    TimeoutInSecs = 0.15,
                },
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableRetryPolicy = resilienceOptions.EnableRetryPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.Timeout.TimeoutInSecs = resilienceOptions.Timeout.TimeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var expectedException = await Should.ThrowAsync<InvalidOperationException>(() =>
            {
                return httpClient
                    .TimeoutExecutor(resilienceOptions.Timeout, testHttpMessageHandler)
                    .TriggerTimeoutPolicyAsync();
            });
            expectedException.Message.ShouldBe("The request should have been aborted but it wasn't. Make sure the HttpClient.Timeout value is set to a value lower than 1.15 seconds.");
        }
    }
}
