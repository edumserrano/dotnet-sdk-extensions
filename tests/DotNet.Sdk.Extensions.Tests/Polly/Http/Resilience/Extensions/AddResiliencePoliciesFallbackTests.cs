using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Http.Resilience;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ResiliencePoliciesHttpClientBuilderExtensions"/> class.
    /// Specifically to test that the fallback policy is added.
    ///
    /// There should be more tests for the conditions that the fallback policy handles. For instance, tests
    /// for when a <see cref="BrokenCircuitException"/> happens the fallback is a <see cref="CircuitBrokenHttpResponseMessage"/>.
    /// However, when all the policies are added together via the AddResiliencePolicies extension method,
    /// it is quite hard to be able to test all of the fallback conditions.
    /// Not to worry much since the AddResiliencePolicies reuses the AddFallbackPolicy which is thoroughly tested
    /// by <see cref="AddFallbackPolicyTests"/>.
    ///
    /// Disables other policies to avoid triggering them when testing the fallback policy. 
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class AddResiliencePoliciesFallbackTests
    {
        /// <summary>
        /// Tests that when requests fail because of a timeout but the fallback policy is disabled
        /// then you do not get a TimeoutHttpResponseMessage, you get a TimeoutRejectedException.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy1()
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

        /// <summary>
        /// Tests that when requests fail because of a timeout but the fallback policy is enabled
        /// then you do not get a TimeoutRejectedException you get a TimeoutHttpResponseMessage.
        /// </summary>
        [Fact]
        public async Task AddResiliencePoliciesAddsFallbackPolicy2()
        {
            var numberOfCallsDelegatingHandler = new NumberOfCallsDelegatingHandler();
            var resiliencePoliciesEventHandlerCalls = new ResiliencePoliciesEventHandlerCalls();
            var testHttpMessageHandler = new TestHttpMessageHandler();
            var httpClientName = "GitHub";
            var resilienceOptions = new ResilienceOptions
            {
                EnableFallbackPolicy = true,
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

            var response = await httpClient.GetAsync(triggerTimeoutPath);
            response.ShouldBeOfType<TimeoutHttpResponseMessage>();
        }
    }
}
