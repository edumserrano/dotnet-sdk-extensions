using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
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
    /// Specifically to test that the retry policy is added.
    /// 
    /// Disables other policies to avoid triggering them when testing the retry policy.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesRetryPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy1()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                }
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                }
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler)
                .Retry
                .HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,Action{ResilienceOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy3()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                }
            };
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Retry.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            resiliencePoliciesAsserter.Retry.EventHandlerShouldReceiveExpectedEvents(
                count: 15 * resilienceOptions.Retry.RetryCount, // the resiliencePoliciesAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Retry);
        }

        /// <summary>
        /// Tests that the <see cref="ResiliencePoliciesHttpClientBuilderExtensions.AddResiliencePolicies{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="IResiliencePoliciesEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy4()
        {
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                }
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(resiliencePoliciesEventHandlerCalls);
            services
                .AddHttpClientResilienceOptions(optionsName)
                .Configure(options =>
                {
                    options.EnableFallbackPolicy = resilienceOptions.EnableFallbackPolicy;
                    options.EnableCircuitBreakerPolicy = resilienceOptions.EnableCircuitBreakerPolicy;
                    options.EnableTimeoutPolicy = resilienceOptions.EnableTimeoutPolicy;
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddResiliencePolicies<TestResiliencePoliciesEventHandler>(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var resiliencePoliciesAsserter = httpClient.ResiliencePoliciesAsserter(resilienceOptions, testHttpMessageHandler);
            await resiliencePoliciesAsserter.Retry.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            resiliencePoliciesAsserter.Retry.EventHandlerShouldReceiveExpectedEvents(
                count: 15 * resilienceOptions.Retry.RetryCount, // the resiliencePoliciesAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: resiliencePoliciesEventHandlerCalls.Retry);
        }

        /// <summary>
        /// Tests that the AddResiliencePolicies method does not add a retry policy if
        /// the <see cref="ResilienceOptions.EnableRetryPolicy"/> option is false.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsRetryPolicy5()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = false,
                EnableRetryPolicy = false,
                EnableCircuitBreakerPolicy = false,
                EnableTimeoutPolicy = false,
                Retry = new RetryOptions
                {
                    RetryCount = 2,
                    MedianFirstRetryDelayInSecs = 0.01
                }
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
                    options.Retry.MedianFirstRetryDelayInSecs = resilienceOptions.Retry.MedianFirstRetryDelayInSecs;
                    options.Retry.RetryCount = resilienceOptions.Retry.RetryCount;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var response = await httpClient
                .RetryExecutor(testHttpMessageHandler)
                .TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode.InternalServerError);
            numberOfCallsDelegatingHandler.NumberOfHttpRequests.ShouldBe(1); // no retries means only 1 http request done even if it failed with a transient http status code
            resilienceOptions.Retry.RetryCount.ShouldNotBe(0); // fail the test if retry is set to zero because it could result in a false positive
        }
    }
}
