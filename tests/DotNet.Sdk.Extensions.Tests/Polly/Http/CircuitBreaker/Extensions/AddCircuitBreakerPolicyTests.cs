using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Wrap;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Extensions
{
    /// <summary>
    /// Tests for the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions"/> class.
    ///
    /// NOTE: when debugging sometimes the tests might not behave as expected because the circuit breaker
    /// is time sensitive in its nature as shown by the duration of break and sampling duration properties.
    /// If required try to increase the CircuitBreakerOptions.SamplingDurationInSecs to a greater value to
    /// allow the tests to run successfully when the debugger is attached. For instance, set it to 2 instead of 0.2.
    ///
    /// This is not ideal but at the moment it's the only suggested workaround because these tests are triggering
    /// the circuit breaker policy and I don't know how to manipulate/fake time for the policy.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddCircuitBreakerPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy1()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler)
                .HttpClientShouldContainCircuitBreakerPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy2()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await httpClient
                .CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler)
                .HttpClientShouldContainCircuitBreakerPolicyAsync();
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="ICircuitBreakerPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy3()
        {
            var circuitBreakerPolicyEventHandlerCalls = new CircuitBreakerPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services.AddSingleton(circuitBreakerPolicyEventHandlerCalls);
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var circuitBreakerAsserter = httpClient.CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler);
            await circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync();
            circuitBreakerAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: circuitBreakerPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="ICircuitBreakerPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy4()
        {
            var circuitBreakerPolicyEventHandlerCalls = new CircuitBreakerPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services.AddSingleton(circuitBreakerPolicyEventHandlerCalls);
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(optionsName)
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var circuitBreakerAsserter = httpClient.CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler);
            await circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync();
            circuitBreakerAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: circuitBreakerPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,string,Func{IServiceProvider,ICircuitBreakerPolicyEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This also tests that the <see cref="ICircuitBreakerPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy5()
        {
            var circuitBreakerPolicyEventHandlerCalls = new CircuitBreakerPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services
                .AddHttpClientCircuitBreakerOptions(optionsName)
                .Configure(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                });
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy(optionsName, provider =>
                {
                    return new TestCircuitBreakerPolicyEventHandler(circuitBreakerPolicyEventHandlerCalls);
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var circuitBreakerAsserter = httpClient.CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler);
            await circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync();
            circuitBreakerAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: circuitBreakerPolicyEventHandlerCalls);
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,Action{CircuitBreakerOptions},Func{IServiceProvider,ICircuitBreakerPolicyEventHandler})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        /// 
        /// This also tests that the <see cref="ICircuitBreakerPolicyEventHandler"/> events are triggered with the correct values.
        /// </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicy6()
        {
            var circuitBreakerPolicyEventHandlerCalls = new CircuitBreakerPolicyEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy(
                    configureOptions: options =>
                    {
                        options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                        options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                        options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                        options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                    },
                    eventHandlerFactory: provider =>
                    {
                        return new TestCircuitBreakerPolicyEventHandler(circuitBreakerPolicyEventHandlerCalls);
                    })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            var circuitBreakerAsserter = httpClient.CircuitBreakerPolicyAsserter(circuitBreakerOptions, testHttpMessageHandler);
            await circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync();
            circuitBreakerAsserter.EventHandlerShouldReceiveExpectedEvents(
                count: 15, // the circuitBreakerAsserter.HttpClientShouldContainCircuitBreakerPolicyAsync triggers the circuit breaker 15 times
                httpClientName: httpClientName,
                eventHandlerCalls: circuitBreakerPolicyEventHandlerCalls);
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
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
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
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");
            circuitBreakerPolicy1.ShouldNotBeNull();
            circuitBreakerPolicy2.ShouldNotBeNull();
            ReferenceEquals(circuitBreakerPolicy1, circuitBreakerPolicy2).ShouldBeFalse();
            circuitBreakerPolicy1.PolicyKey.ShouldNotBe(circuitBreakerPolicy2.PolicyKey);
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// CircuitBreakerHttpClientBuilderExtensions.AddCircuitBreakerPolicy methods do not trigger
        /// an exception if the circuit state is open or isolated.
        ///
        /// This is because the policy added is a wrapped policy which joins an <see cref="AsyncCircuitBreakerPolicy{T}"/>
        /// and a <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/>.
        /// The <see cref="CircuitBreakerCheckerAsyncPolicy{T}"/>. will check if the circuit is open/isolated and
        /// if so it will return <see cref="CircuitBrokenHttpResponseMessage"/> which is an http response message
        /// with 500 status code and some extra properties.
        ///
        /// This is to optimize the performance of the code by reducing the exceptions thrown as indicated by
        /// https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken 
        ///  </summary>
        [Fact]
        public async Task AddCircuitBreakerPolicyDoesNotThrowExceptionWhenCircuitIsOpen()
        {
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 0.01,
                SamplingDurationInSecs = 0.08,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigurePrimaryHttpMessageHandler(() => testHttpMessageHandler);

            await using var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient("GitHub");
            var circuitBreaker = httpClient.CircuitBreakerExecutor(circuitBreakerOptions, testHttpMessageHandler);
            await circuitBreaker.TriggerFromTransientHttpStatusCodeAsync(HttpStatusCode.ServiceUnavailable);
            // after the above the circuit state is now open which means the following http request, if it hit the
            // circuit breaker policy, would throw a BrokenCircuitException/IsolatedCircuitException; however,
            // because we wrapped the circuit breaker policy with a CircuitBreakerCheckerAsyncPolicy what we get
            // instead is a CircuitBrokenHttpResponseMessage instance
            var response = await httpClient.GetAsync($"/circuit-breaker/transient-http-status-code/{HttpStatusCode.ServiceUnavailable}");
            var circuitBrokenHttpResponseMessage = response as CircuitBrokenHttpResponseMessage;
            circuitBrokenHttpResponseMessage.ShouldNotBeNull();
            circuitBrokenHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            circuitBrokenHttpResponseMessage.CircuitBreakerState.ShouldBe(CircuitBreakerState.Open);
            // the Exception property should be null because the circuit breaker should NOT be throwing an exception.
            // The CircuitBreakerCheckerAsyncPolicy should check that the circuit is open and return the
            // CircuitBrokenHttpResponseMessage without any exception
            circuitBrokenHttpResponseMessage.Exception.ShouldBeNull();
        }
    }
}
