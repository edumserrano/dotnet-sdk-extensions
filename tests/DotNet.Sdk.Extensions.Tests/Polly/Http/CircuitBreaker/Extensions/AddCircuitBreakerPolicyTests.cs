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
    [Collection(XUnitTestCollections.CircuitBreakerPolicy)]
    public class AddCircuitBreakerPolicyTests : IDisposable
    {
        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a circuit break to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="CircuitBreakerOptions"/>.
        /// </summary>
        [Fact]
        public void AddCircuitBreakerPolicy1()
        {
            AsyncPolicyWrap<HttpResponseMessage>? circuitBreakerPolicy = null;
            var httpClientName = "GitHub";
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var circuitBreakerAsserter = new CircuitBreakerPolicyAsserter(
                httpClientName,
                circuitBreakerOptions,
                circuitBreakerPolicy);
            circuitBreakerAsserter.PolicyShouldBeConfiguredAsExpected();
            circuitBreakerAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy(IHttpClientBuilder,string)"/>
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
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
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
                .AddCircuitBreakerPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var circuitBreakerAsserter = new CircuitBreakerPolicyAsserter(
                httpClientName,
                circuitBreakerOptions,
                circuitBreakerPolicy);
            circuitBreakerAsserter.PolicyShouldBeConfiguredAsExpected();
            circuitBreakerAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{CircuitBreakerOptions})"/>
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
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
                FailureThreshold = 0.6,
                MinimumThroughput = 10
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(options =>
                {
                    options.DurationOfBreakInSecs = circuitBreakerOptions.DurationOfBreakInSecs;
                    options.SamplingDurationInSecs = circuitBreakerOptions.SamplingDurationInSecs;
                    options.FailureThreshold = circuitBreakerOptions.FailureThreshold;
                    options.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var circuitBreakerAsserter = new CircuitBreakerPolicyAsserter(
                httpClientName,
                circuitBreakerOptions,
                circuitBreakerPolicy);
            circuitBreakerAsserter.PolicyShouldBeConfiguredAsExpected();
            circuitBreakerAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestCircuitBreakerPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="CircuitBreakerPolicyHttpClientBuilderExtensions.AddCircuitBreakerPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
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
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
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
                .AddCircuitBreakerPolicy<TestCircuitBreakerPolicyEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    circuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var circuitBreakerAsserter = new CircuitBreakerPolicyAsserter(
                httpClientName,
                circuitBreakerOptions,
                circuitBreakerPolicy);
            circuitBreakerAsserter.PolicyShouldBeConfiguredAsExpected();
            circuitBreakerAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestCircuitBreakerPolicyEventHandler));
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
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
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

            var serviceProvider = services.BuildServiceProvider();
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
            AsyncPolicyWrap<HttpResponseMessage>? wrappedCircuitBreakerPolicy = null;
            var circuitBreakerOptions = new CircuitBreakerOptions
            {
                DurationOfBreakInSecs = 30,
                SamplingDurationInSecs = 60,
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
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            builder.RespondWith(httpRequestMessage =>
                            {
                                // return HttpStatusCode.ServiceUnavailable to trigger the circuit breaker
                                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                            });
                        });
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    wrappedCircuitBreakerPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncPolicyWrap<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient("GitHub");

            wrappedCircuitBreakerPolicy.ShouldNotBeNull();
            var circuitBreakerPolicy = (AsyncCircuitBreakerPolicy<HttpResponseMessage>)wrappedCircuitBreakerPolicy.Inner;

            // isolate the circuit and do a request to check it does not throw an exception
            circuitBreakerPolicy.Isolate();
            var circuitBrokenHttpResponseMessage1 = (CircuitBrokenHttpResponseMessage)await httpClient.GetAsync("http://github.com");
            circuitBrokenHttpResponseMessage1.StatusCode.ShouldBe(HttpStatusCode.InternalServerError); 
            circuitBrokenHttpResponseMessage1.CircuitBreakerState.ShouldBe(CircuitBreakerState.Isolated);
            circuitBrokenHttpResponseMessage1.BrokenCircuitException.ShouldBeNull();
            circuitBrokenHttpResponseMessage1.IsolatedCircuitException.ShouldBeNull();

            // reset and trigger the circuit breaker policy so that the circuit opens then send a request
            // which should also not throw an exception
            circuitBreakerPolicy.Reset();
            for (var i = 0; i < circuitBreakerOptions.MinimumThroughput; i++)
            {
                var response = await httpClient.GetAsync("https://github.com");
                // just to show that the status code that is being return is HttpStatusCode.ServiceUnavailable
                // and that will trigger the circuit after this while loop is done. Then the status code
                // will be a HttpStatusCode.InternalServerError just to indicate that request failed, it was a quick
                // fail because the circuit is open, the request was not even attempted and it didn't reach the circuit
                // breaker or else an exception would have been thrown
                response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
            }
            var circuitBrokenHttpResponseMessage2 = (CircuitBrokenHttpResponseMessage)await httpClient.GetAsync("http://github.com");
            circuitBrokenHttpResponseMessage2.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            circuitBrokenHttpResponseMessage2.CircuitBreakerState.ShouldBe(CircuitBreakerState.Open);
            circuitBrokenHttpResponseMessage2.BrokenCircuitException.ShouldBeNull();
            circuitBrokenHttpResponseMessage2.IsolatedCircuitException.ShouldBeNull();
        }

        public void Dispose()
        {
            TestCircuitBreakerPolicyEventHandler.Clear();
        }
    }
}
