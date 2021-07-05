using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TimeoutPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically for the TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy overloads. 
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddTimeoutPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,Action{TimeoutOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy1()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler)
                .HttpClientShouldContainTimeoutPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy2()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutOptions.TimeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler)
                .HttpClientShouldContainTimeoutPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{TimeoutOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="ITimeoutPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy3()
        {
            var timeoutPolicyEventHandlerCalls = new TimeoutPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var services = new ServiceCollection();
            services.AddSingleton(timeoutPolicyEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var timeoutPolicyAsserter = httpClient.TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler);
            await timeoutPolicyAsserter.HttpClientShouldContainTimeoutPolicyAsync();
            timeoutPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: timeoutPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="ITimeoutPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy4()
        {
            var timeoutPolicyEventHandlerCalls = new TimeoutPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services.AddSingleton(timeoutPolicyEventHandlerCalls);
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutOptions.TimeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var timeoutPolicyAsserter = httpClient.TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler);
            await timeoutPolicyAsserter.HttpClientShouldContainTimeoutPolicyAsync();
            timeoutPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: timeoutPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,string,Func{IServiceProvider,ITimeoutPolicyEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="ITimeoutPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy5()
        {
            var timeoutPolicyEventHandlerCalls = new TimeoutPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutOptions.TimeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy(optionsName, provider =>
                {
                    return new TestTimeoutPolicyEventHandler(timeoutPolicyEventHandlerCalls);
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var timeoutPolicyAsserter = httpClient.TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler);
            await timeoutPolicyAsserter.HttpClientShouldContainTimeoutPolicyAsync();
            timeoutPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: timeoutPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,Action{TimeoutOptions},Func{IServiceProvider,ITimeoutPolicyEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="ITimeoutPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicy6()
        {
            var timeoutPolicyEventHandlerCalls = new TimeoutPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var timeoutOptions = new TimeoutOptions
            {
                TimeoutInSecs = 0.05
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddTimeoutPolicy(
                    configureOptions: options =>
                    {
                        options.TimeoutInSecs = timeoutOptions.TimeoutInSecs;
                    },
                    eventHandlerFactory: provider =>
                    {
                        return new TestTimeoutPolicyEventHandler(timeoutPolicyEventHandlerCalls);
                    })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var timeoutPolicyAsserter = httpClient.TimeoutPolicyAsserter(timeoutOptions, testHttpMessageHandler);
            await timeoutPolicyAsserter.HttpClientShouldContainTimeoutPolicyAsync();
            timeoutPolicyAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 1,
                httpClientName: httpClientName,
                eventHandlerCalls: timeoutPolicyEventHandlerCalls);
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyUniquePolicyPerHttpClient()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy1 = null;
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 1;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 2;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            timeoutPolicy1.ShouldNotBeNull();
            timeoutPolicy2.ShouldNotBeNull();
            ReferenceEquals(timeoutPolicy1, timeoutPolicy2).ShouldBeFalse();
            timeoutPolicy1.PolicyKey.ShouldNotBe(timeoutPolicy2.PolicyKey);
        }
    }
}
