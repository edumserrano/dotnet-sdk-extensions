using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddRetryPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddRetryPolicy1()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClient,
                retryOptions,
                testHttpMessageHandler);
            await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>. 
        /// </summary>
        [Fact]
        public async Task AddRetryPolicy2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddRetryPolicy(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClient,
                retryOptions,
                testHttpMessageHandler);
            await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the  <see cref="IRetryPolicyEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddRetryPolicy3()
        {
            var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01
            };
            var services = new ServiceCollection();
            services.AddSingleton(retryPolicyEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddRetryPolicy<TestRetryPolicyEventHandler>(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                })
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClient,
                retryOptions,
                testHttpMessageHandler);
            await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 4 times
                httpClientName: httpClientName,
                options: retryOptions,
                eventHandlerCalls: retryPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the  <see cref="IRetryPolicyEventHandler.OnRetryAsync"/> is triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddRetryPolicy4()
        {
            var retryPolicyEventHandlerCalls = new RetryPolicyEventHandlerCalls();
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 0.01
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(retryPolicyEventHandlerCalls);
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddRetryPolicy<TestRetryPolicyEventHandler>(optionsName)
                .AddHttpMessageHandler(() => numberOfCallsDelegatingHandler)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClient,
                retryOptions,
                testHttpMessageHandler);
            await retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync(numberOfCallsDelegatingHandler);
            retryPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15 * retryOptions.RetryCount, // the retryPolicyAsserter.HttpClientShouldContainRetryPolicyAsync triggers the retry policy 4 times
                httpClientName: httpClientName,
                options: retryOptions,
                eventHandlerCalls: retryPolicyEventHandlerCalls);
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddRetryPolicyUniquePolicyPerHttpClient()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy1 = null;
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 2;
                    options.MedianFirstRetryDelayInSecs = 0.01;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 3;
                    options.MedianFirstRetryDelayInSecs = 0.02;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            retryPolicy1.ShouldNotBeNull();
            retryPolicy2.ShouldNotBeNull();
            ReferenceEquals(retryPolicy1, retryPolicy2).ShouldBeFalse();
            retryPolicy1.PolicyKey.ShouldNotBe(retryPolicy2.PolicyKey);
        }
    }
}
