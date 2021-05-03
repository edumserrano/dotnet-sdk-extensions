using System;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
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
    public class AddTimeoutPolicyTests
    {
        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,Action{TimeoutOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="TimeoutOptions"/>.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy1()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyEventHandler: typeof(DefaultTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts only the name of the option to use for the value of the <see cref="TimeoutOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy2()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyEventHandler: typeof(DefaultTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{TimeoutOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts the name of the option to use for the value of the <see cref="TimeoutOptions"/>
        /// and a <see cref="ITimeoutPolicyEventHandler"/> type to handle the timeout policy events.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy3()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyEventHandler: typeof(TestTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="TimeoutPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts the name of the option to use for the value of the <see cref="TimeoutOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="TimeoutOptionsExtensions.AddHttpClientTimeoutOptions"/> extension method.
        ///
        /// This overload also accepts an <see cref="ITimeoutPolicyEventHandler"/> type to handle the timeout policy events.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy4()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services
                .AddHttpClientTimeoutOptions(optionsName)
                .Configure(options => options.TimeoutInSecs = timeoutInSecs);
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyEventHandler: typeof(TestTimeoutPolicyEventHandler));
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
